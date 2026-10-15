using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace XeBus_GIS.UI
{
    /// <summary>
    /// Chuyển chế độ sáng/tối khi đang chạy (không khởi động lại, không đụng dữ liệu/bản đồ):
    /// 1. <see cref="Theme.ApDung"/> đổi giá trị token.
    /// 2. Duyệt cây control của các form đang mở MỘT lần: màu nào trùng giá trị token cũ thì đổi sang token mới
    ///    (BackColor theo nhóm màu nền, ForeColor theo nhóm màu chữ) – bao phủ màu đặt trong Designer và ApDungGiaoDien.
    /// 3. Chạy lại các hành động đã đăng ký (<see cref="DangKy"/>) cho control có kiểu dáng phức tạp: nút (hover/pressed),
    ///    lưới (cell style), biểu đồ, icon vẽ sẵn theo màu…
    /// 4. Khoá vẽ cả form trong lúc đổi (WM_SETREDRAW) rồi vẽ lại một lần – không nhấp nháy.
    /// Control tự vẽ đọc Theme.X trong OnPaint nên chỉ cần vẽ lại. Bỏ qua ActiveX (bản đồ MapWinGIS – đổi riêng ở
    /// MapManager.ApDungTheme) và ElementHost (WPF).
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>Sau khi đổi chế độ (đã áp lên control) – form chính cập nhật bản đồ, thanh trạng thái…</summary>
        public static event Action DaDoi;

        private class MucDangKy
        {
            public WeakReference<Control> C;
            public string Khoa;
            public Action ApDung;
        }

        private static readonly List<MucDangKy> _ds = new List<MucDangKy>();

        /// <summary>
        /// Đăng ký hành động áp màu cho control (chạy lại khi đổi chế độ). Cùng (control, khoá) chỉ giữ hành động mới nhất.
        /// </summary>
        public static void DangKy(Control c, string khoa, Action apDung)
        {
            if (c == null || apDung == null) return;
            for (int i = _ds.Count - 1; i >= 0; i--)
            {
                if (!_ds[i].C.TryGetTarget(out var x) || x.IsDisposed) { _ds.RemoveAt(i); continue; }
                if (x == c && _ds[i].Khoa == khoa) { _ds[i].ApDung = apDung; return; }
            }
            _ds.Add(new MucDangKy { C = new WeakReference<Control>(c), Khoa = khoa, ApDung = apDung });
        }

        // Token dùng làm màu nền / màu chữ (ánh xạ riêng để chữ trắng trên nút không bị đổi thành màu nền tối)
        private static readonly string[] TokenNen =
        {
            "Surface", "Background", "SurfaceAlt", "SurfaceHover", "Sidebar", "SidebarHover", "SidebarChon", "AccentSoft",
            "AccentSoftHover", "RowAlt", "HeaderHover", "Border", "BorderStrong", "Accent", "AccentHover", "DangerSoft",
        };
        private static readonly string[] TokenChu =
        {
            "Text", "Primary", "Secondary", "Muted", "Disabled", "Accent", "Danger", "Success", "WarningText", "ChuTrenAccent",
        };

        /// <summary>Đổi chế độ sáng/tối và áp lên mọi form đang mở; lưu lựa chọn cho lần mở sau.</summary>
        public static void DoiCheDo(bool toi)
        {
            if (toi == Theme.Toi) return;
            var cu = Theme.ChupBang();
            Theme.ApDung(toi);
            var moi = Theme.ChupBang();
            CaiDatNguoiDung.Ghi(CaiDatNguoiDung.KhoaTheme, toi ? "toi" : "sang");

            var mapNen = TaoBang(cu, moi, TokenNen);
            var mapChu = TaoBang(cu, moi, TokenChu);
            // màu Designer/WinForms mặc định của nền
            mapNen[Color.White.ToArgb()] = Theme.Surface;
            mapNen[SystemColors.Window.ToArgb()] = Theme.Surface;
            mapNen[SystemColors.Control.ToArgb()] = Theme.Background;
            mapChu[SystemColors.ControlText.ToArgb()] = Theme.Text;
            mapChu[SystemColors.WindowText.ToArgb()] = Theme.Text;

            var forms = Application.OpenForms.Cast<Form>().ToList();
            foreach (var f in forms) KhoaVe(f, true);
            try
            {
                foreach (var f in forms)
                {
                    DoiMauCay(f, mapNen, mapChu);
                    TieuDeToi(f);
                }
                foreach (var m in _ds.ToList())
                {
                    if (!m.C.TryGetTarget(out var c) || c.IsDisposed) continue;
                    try { m.ApDung(); } catch { /* không để một control làm hỏng cả lượt đổi */ }
                }
                DaDoi?.Invoke();
            }
            finally
            {
                foreach (var f in forms)
                {
                    KhoaVe(f, false);
                    f.Invalidate(true);
                }
            }
        }

        private static Dictionary<int, Color> TaoBang(Dictionary<string, Color> cu, Dictionary<string, Color> moi, string[] token)
        {
            var kq = new Dictionary<int, Color>();
            foreach (var t in token)
            {
                if (!cu.TryGetValue(t, out var a) || !moi.TryGetValue(t, out var b)) continue;
                int k = a.ToArgb();
                if (!kq.ContainsKey(k)) kq[k] = b;   // token đứng trước được ưu tiên khi trùng giá trị
            }
            return kq;
        }

        private static void DoiMauCay(Control c, Dictionary<int, Color> mapNen, Dictionary<int, Color> mapChu)
        {
            if (c is AxHost || c.GetType().Name == "ElementHost") return;
            try
            {
                if (mapNen.TryGetValue(c.BackColor.ToArgb(), out var n) && c.BackColor.A == 255) c.BackColor = n;
                if (mapChu.TryGetValue(c.ForeColor.ToArgb(), out var t)) c.ForeColor = t;
                if (c is DataGridView g)
                {
                    if (mapNen.TryGetValue(g.BackgroundColor.ToArgb(), out var bg)) g.BackgroundColor = bg;
                    if (mapNen.TryGetValue(g.GridColor.ToArgb(), out var gc)) g.GridColor = gc;
                }
                if (c is ToolStrip ts)
                    foreach (ToolStripItem it in ts.Items) DoiMauMuc(it, mapNen, mapChu);
            }
            catch { /* control không cho đổi màu – bỏ qua */ }
            if (c.ContextMenuStrip != null)
                foreach (ToolStripItem it in c.ContextMenuStrip.Items) DoiMauMuc(it, mapNen, mapChu);
            foreach (Control con in c.Controls) DoiMauCay(con, mapNen, mapChu);
        }

        private static void DoiMauMuc(ToolStripItem it, Dictionary<int, Color> mapNen, Dictionary<int, Color> mapChu)
        {
            if (mapNen.TryGetValue(it.BackColor.ToArgb(), out var n)) it.BackColor = n;
            if (mapChu.TryGetValue(it.ForeColor.ToArgb(), out var t)) it.ForeColor = t;
            if (it is ToolStripDropDownItem dd)
                foreach (ToolStripItem con in dd.DropDownItems) DoiMauMuc(con, mapNen, mapChu);
        }

        // =====================================================================
        // Win32: khoá vẽ, thanh tiêu đề tối
        // =====================================================================
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, IntPtr l);
        [DllImport("user32.dll")] private static extern bool RedrawWindow(IntPtr h, IntPtr r, IntPtr rgn, uint flags);
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr h, int attr, ref int val, int size);

        /// <summary>Tạm ngừng / cho vẽ lại control (WM_SETREDRAW) – đổi nhiều thuộc tính mà không nhấp nháy.</summary>
        public static void KhoaVe(Control c, bool khoa)
        {
            if (c == null || !c.IsHandleCreated) return;
            SendMessage(c.Handle, 0x000B, khoa ? IntPtr.Zero : (IntPtr)1, IntPtr.Zero);
            if (!khoa) RedrawWindow(c.Handle, IntPtr.Zero, IntPtr.Zero, 0x0001 | 0x0004 | 0x0080 | 0x0100); // INVALIDATE|ERASE|ALLCHILDREN|FRAME
        }

        /// <summary>Thanh tiêu đề cửa sổ theo chế độ (Windows 10 2004+/11; bản cũ hơn bỏ qua).</summary>
        public static void TieuDeToi(Form f)
        {
            if (f == null) return;
            void Dat()
            {
                try
                {
                    int bat = Theme.Toi ? 1 : 0;
                    if (DwmSetWindowAttribute(f.Handle, 20, ref bat, 4) != 0) DwmSetWindowAttribute(f.Handle, 19, ref bat, 4);
                }
                catch { /* hệ điều hành không hỗ trợ */ }
            }
            if (f.IsHandleCreated) Dat();
            else f.HandleCreated += (s, e) => Dat();
        }
    }
}
