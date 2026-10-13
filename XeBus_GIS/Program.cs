using System;
using System.Windows.Forms;
using XeBus_GIS.Forms;

namespace XeBus_GIS
{
    internal static class Program
    {
        /// <summary>
        /// Điểm vào: Đăng nhập -> Form chính; Đăng xuất quay lại Đăng nhập.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (s, e) =>
                MessageBox.Show(e.Exception.GetBaseException().Message, "Lỗi không mong muốn", MessageBoxButtons.OK, MessageBoxIcon.Error);

            while (true)
            {
                using (var dn = new FrmDangNhap())
                {
                    if (dn.ShowDialog() != DialogResult.OK) return;
                }
                using (var main = new FrmMain())
                {
                    Application.Run(main);
                    if (!main.DaDangXuat) return;
                }
            }
        }
    }
}
