using System.Drawing;
using System.Windows.Forms;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>Module tạm cho các chức năng chưa xây dựng.</summary>
    public class UcDangPhatTrien : UserControl, IModule
    {
        public UcDangPhatTrien(string tieuDe, string moTa)
        {
            BackColor = Theme.Surface;
            Padding = new Padding(16);
            var lblTieuDe = new Label { Text = tieuDe, Dock = DockStyle.Top, Height = 32 };
            UiHelper.TieuDe(lblTieuDe);
            lblTieuDe.AutoSize = false;
            var lblMoTa = new Label
            {
                Text = moTa,
                Dock = DockStyle.Top,
                Height = 80,
                ForeColor = Theme.Muted,
                Font = Theme.FontThuong,
            };
            Controls.Add(lblMoTa);
            Controls.Add(lblTieuDe);
        }

        public void KichHoat() { }
        public void NgungKichHoat() { }
    }
}
