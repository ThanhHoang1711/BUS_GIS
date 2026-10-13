using System;
using System.Security.Cryptography;

namespace XeBus_GIS.Security
{
    /// <summary>
    /// Băm mật khẩu PBKDF2-HMAC-SHA256. Định dạng lưu: "&lt;vòng lặp&gt;:&lt;salt base64&gt;:&lt;hash base64&gt;"
    /// (khớp database/05_baomat.sql).
    /// </summary>
    public static class PasswordHasher
    {
        private const int SoVongLap = 10000;
        private const int DoDaiSalt = 16;
        private const int DoDaiHash = 32;

        public static string Hash(string matKhau)
        {
            var salt = new byte[DoDaiSalt];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            var hash = Pbkdf2(matKhau, salt, SoVongLap, DoDaiHash);
            return SoVongLap + ":" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool Verify(string matKhau, string chuoiLuu)
        {
            if (string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(chuoiLuu)) return false;
            var phan = chuoiLuu.Split(':');
            if (phan.Length != 3 || !int.TryParse(phan[0], out int vong)) return false;
            try
            {
                var salt = Convert.FromBase64String(phan[1]);
                var hashLuu = Convert.FromBase64String(phan[2]);
                var hash = Pbkdf2(matKhau, salt, vong, hashLuu.Length);
                return SoSanhHangThoiGian(hash, hashLuu);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] Pbkdf2(string matKhau, byte[] salt, int vong, int doDai)
        {
            using (var kdf = new Rfc2898DeriveBytes(matKhau, salt, vong, HashAlgorithmName.SHA256))
                return kdf.GetBytes(doDai);
        }

        private static bool SoSanhHangThoiGian(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int khac = 0;
            for (int i = 0; i < a.Length; i++) khac |= a[i] ^ b[i];
            return khac == 0;
        }
    }
}
