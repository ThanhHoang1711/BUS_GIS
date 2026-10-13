using System;
using System.Collections.Generic;

namespace XeBus_GIS.Security
{
    /// <summary>Phiên đăng nhập hiện tại và tập quyền của nhóm người dùng.</summary>
    public static class Session
    {
        public static int? IdNguoiDung { get; private set; }
        public static string TenDangNhap { get; private set; }
        public static string TenNguoiDung { get; private set; }
        public static int IdNhom { get; private set; }
        public static string TenNhom { get; private set; }
        public static long? IdLichSuDangNhap { get; set; }

        /// <summary>Tập "MACHUCNANG:LOAITHAOTAC".</summary>
        private static readonly HashSet<string> _quyen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _chucNang = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static bool DaDangNhap => TenNhom != null;
        public static bool LaKhach => IdNguoiDung == null;

        public static void BatDau(int? idNguoiDung, string tenDangNhap, string tenNguoiDung, int idNhom, string tenNhom,
                                  IEnumerable<KeyValuePair<string, string>> quyen)
        {
            IdNguoiDung = idNguoiDung;
            TenDangNhap = tenDangNhap;
            TenNguoiDung = tenNguoiDung;
            IdNhom = idNhom;
            TenNhom = tenNhom;
            _quyen.Clear();
            _chucNang.Clear();
            foreach (var q in quyen)
            {
                _quyen.Add(q.Key + ":" + q.Value);
                _chucNang.Add(q.Key);
            }
        }

        public static void KetThuc()
        {
            IdNguoiDung = null;
            TenDangNhap = TenNguoiDung = TenNhom = null;
            IdNhom = 0;
            IdLichSuDangNhap = null;
            _quyen.Clear();
            _chucNang.Clear();
        }

        /// <summary>Có quyền với chức năng (bất kỳ thao tác nào, hoặc đúng thao tác nếu truyền vào).</summary>
        public static bool Co(string maChucNang, string thaoTac = null)
            => thaoTac == null ? _chucNang.Contains(maChucNang) : _quyen.Contains(maChucNang + ":" + thaoTac);

        /// <summary>Có ít nhất một chức năng bắt đầu bằng tiền tố (vd "DX_", "QT_").</summary>
        public static bool CoNhom(string tienTo)
        {
            foreach (var c in _chucNang)
                if (c.StartsWith(tienTo, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
