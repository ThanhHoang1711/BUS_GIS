using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using XeBus_GIS.Data;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Quy trình 2.5 – Gán trạm vào tuyến xe buýt: chọn trạm (tìm kiếm hoặc click trên bản đồ),
    /// sắp xếp thứ tự phục vụ, lưu vào tuyen_tram và (tuỳ chọn) dựng lại đường đi tuyến.
    /// Cửa sổ không chặn (Show) để người dùng vẫn thao tác được trên bản đồ.
    /// </summary>
    public partial class FrmGanTram : Form
    {
        private readonly string _maTuyen;
        private readonly DataTable _ds;              // Mã, Tên trạm, Phường/xã, wkt – theo thứ tự trên tuyến
        private readonly Action<DataTable> _xemTruoc; // vẽ thứ tự trạm lên bản đồ
        private bool _thayDoi;
        private bool _daLuu;

        /// <summary>Phát ra sau khi lưu thành công.</summary>
        public event Action DaLuu;

        /// <summary>Dùng cho VS Designer.</summary>
        public FrmGanTram()
        {
            InitializeComponent();
            UI.UiHelper.CoGianDpi(this);   // co giãn theo DPI ngay – trước khi code dựng thêm control bằng UiHelper.Dp()
        }

        public FrmGanTram(DataRow tuyen, DataTable tramTrenTuyen, Action<DataTable> xemTruoc) : this()
        {
            _maTuyen = Db.Str(tuyen, "matuyen");
            _xemTruoc = xemTruoc;
            _ds = new DataTable();
            _ds.Columns.Add("#", typeof(int));
            _ds.Columns.Add("Mã");
            _ds.Columns.Add("Tên trạm");
            _ds.Columns.Add("Phường/xã");
            _ds.Columns.Add("wkt");
            foreach (DataRow r in tramTrenTuyen.Rows)
                _ds.Rows.Add(0, r["Mã"], r["Tên trạm"], r["Phường/xã"], r["wkt"]);

            ApDungGiaoDien();
            lblTieuDe.Text = $"Gán trạm vào tuyến {_maTuyen} – {Db.Str(tuyen, "tentuyen")}";
            dgvDich.DataSource = _ds;
            DinhDangLuoiDich();
            DanhSoLai();
            GanSuKien();
        }

        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;
            UiHelper.ChuanHoaO(this);
            ThemeManager.TieuDeToi(this);
            lblTieuDe.Font = Theme.FontTieuDe;
            lblTieuDe.ForeColor = Theme.Primary;
            lblGoiY.ForeColor = Theme.Muted;
            lblGoiY.Font = Theme.FontNhan;
            foreach (var l in new[] { lblNguon, lblDich })
            {
                UiHelper.TieuDeKhuVuc(l);
            }
            UiHelper.Luoi(dgvNguon);
            dgvNguon.MultiSelect = true;
            UiHelper.Luoi(dgvDich);
            dgvDich.MultiSelect = true;
            UiHelper.NutChinh(btnTim);
            UiHelper.NutIcon(btnThem, Icons.Phai, "Thêm trạm đã chọn vào tuyến (sau trạm đang chọn bên phải)", 14f);
            UiHelper.NutIcon(btnBo, Icons.Trai, "Bỏ trạm đã chọn khỏi tuyến", 14f);
            UiHelper.NutIcon(btnLen, Icons.Len, "Chuyển trạm lên trước", 14f);
            UiHelper.NutIcon(btnXuong, Icons.Xuong, "Chuyển trạm xuống sau", 14f);
            foreach (var b in new[] { btnThem, btnBo, btnLen, btnXuong })
            {
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.BorderColor = Theme.Border;
            }
            chkDungLai.ForeColor = Theme.Text;
            chkDungLai.Cursor = Cursors.Hand;
            lblThayDoi.Font = Theme.FontNhan;
            UiHelper.NutChinh(btnLuu);
            UiHelper.NutPhu(btnHuy);
        }

        private void DinhDangLuoiDich()
        {
            dgvDich.Columns["wkt"].Visible = false;
            dgvDich.Columns["Phường/xã"].Visible = false;
            dgvDich.Columns["#"].FillWeight = 14;
            dgvDich.Columns["Mã"].FillWeight = 26;
            dgvDich.Columns["Tên trạm"].FillWeight = 80;
            foreach (DataGridViewColumn c in dgvDich.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            UiHelper.CanPhaiCotSo(dgvDich);
        }

        private void GanSuKien()
        {
            btnTim.Click += async (s, e) => await TimAsync();
            txtTim.KeyDown += async (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                await TimAsync();
            };
            btnThem.Click += async (s, e) => await ThemTuNguonAsync();
            dgvNguon.CellDoubleClick += async (s, e) => { if (e.RowIndex >= 0) await ThemTuNguonAsync(); };
            btnBo.Click += (s, e) => BoDaChon();
            dgvDich.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) BoDaChon(); };
            btnLen.Click += (s, e) => DiChuyen(-1);
            btnXuong.Click += (s, e) => DiChuyen(+1);
            btnLuu.Click += async (s, e) => await LuuAsync();
            btnHuy.Click += (s, e) => Close();
            FormClosing += (s, e) =>
            {
                if (_thayDoi && !_daLuu && !UiHelper.XacNhanNguyHiem(this, "Danh sách trạm đã thay đổi nhưng chưa lưu. Đóng và bỏ các thay đổi này?", "Bỏ thay đổi"))
                    e.Cancel = true;
            };
        }

        // ------------------------------------------------------------------
        // Danh sách nguồn
        // ------------------------------------------------------------------
        private async Task TimAsync()
        {
            var tk = txtTim.Text;
            if (string.IsNullOrWhiteSpace(tk)) return;
            DataTable kq = null;
            if (!await UiHelper.ChayAsync(btnTim, async () => kq = await Task.Run(() => TramRepository.TraCuuThuocTinh(tk, null, null, null, null)), "…"))
                return;
            dgvNguon.DataSource = kq;
            foreach (var c in new[] { "Loại", "Trạng thái", "Tuyến" })
                if (dgvNguon.Columns.Contains(c)) dgvNguon.Columns[c].Visible = false;
            if (dgvNguon.Columns.Contains("Mã")) dgvNguon.Columns["Mã"].FillWeight = 26;
            if (dgvNguon.Columns.Contains("Tên trạm")) dgvNguon.Columns["Tên trạm"].FillWeight = 70;
            if (dgvNguon.Columns.Contains("Phường/xã")) dgvNguon.Columns["Phường/xã"].FillWeight = 44;
            lblNguon.Text = $"Tìm trạm – {kq.Rows.Count} kết quả";
        }

        private async Task ThemTuNguonAsync()
        {
            var ma = dgvNguon.SelectedRows.Cast<DataGridViewRow>()
                              .OrderBy(r => r.Index)
                              .Select(r => Convert.ToString(r.Cells["Mã"].Value)).ToArray();
            await ThemAsync(ma, sauDongDangChon: true);
        }

        /// <summary>Thêm trạm click trên bản đồ vào cuối tuyến (gọi từ UcTuyen).</summary>
        public async Task ThemTramAsync(string maTram)
        {
            await ThemAsync(new[] { maTram }, sauDongDangChon: false);
            Activate();
        }

        private async Task ThemAsync(string[] danhSachMa, bool sauDongDangChon)
        {
            var co = new HashSet<string>(_ds.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["Mã"])));
            var moi = danhSachMa.Where(m => !string.IsNullOrEmpty(m) && !co.Contains(m)).Distinct().ToArray();
            if (moi.Length == 0)
            {
                BaoThayDoi(danhSachMa.Length == 1 ? $"Trạm {danhSachMa[0]} đã có trên tuyến." : "Các trạm đã chọn đều đã có trên tuyến.", true);
                return;
            }
            DataTable tt;
            try
            {
                tt = await Task.Run(() => TuyenRepository.ThongTinTram(moi));
            }
            catch (Exception ex)
            {
                UiHelper.Loi(this, ex.GetBaseException().Message);
                return;
            }
            int viTri = sauDongDangChon && dgvDich.CurrentRow != null ? dgvDich.CurrentRow.Index + 1 : _ds.Rows.Count;
            foreach (DataRow r in tt.Rows)
            {
                var row = _ds.NewRow();
                row["Mã"] = r["Mã"];
                row["Tên trạm"] = r["Tên trạm"];
                row["Phường/xã"] = r["Phường/xã"];
                row["wkt"] = r["wkt"];
                _ds.Rows.InsertAt(row, viTri++);
            }
            DaThayDoi($"Đã thêm {tt.Rows.Count} trạm.");
            ChonDong(viTri - 1);
        }

        // ------------------------------------------------------------------
        // Sắp xếp danh sách đích
        // ------------------------------------------------------------------
        private void BoDaChon()
        {
            var dong = dgvDich.SelectedRows.Cast<DataGridViewRow>().Select(r => r.Index).OrderByDescending(i => i).ToList();
            if (dong.Count == 0) return;
            foreach (var i in dong) _ds.Rows.RemoveAt(i);
            DaThayDoi($"Đã bỏ {dong.Count} trạm.");
            if (_ds.Rows.Count > 0) ChonDong(Math.Min(dong.Last(), _ds.Rows.Count - 1));
        }

        private void DiChuyen(int huong)
        {
            if (dgvDich.CurrentRow == null) return;
            int i = dgvDich.CurrentRow.Index, j = i + huong;
            if (j < 0 || j >= _ds.Rows.Count) return;
            var giaTri = _ds.Rows[i].ItemArray;
            _ds.Rows.RemoveAt(i);
            var row = _ds.NewRow();
            row.ItemArray = giaTri;
            _ds.Rows.InsertAt(row, j);
            DaThayDoi(null);
            ChonDong(j);
        }

        private void ChonDong(int i)
        {
            if (i < 0 || i >= dgvDich.Rows.Count) return;
            dgvDich.ClearSelection();
            dgvDich.CurrentCell = dgvDich.Rows[i].Cells["Mã"];
            dgvDich.Rows[i].Selected = true;
        }

        private void DanhSoLai()
        {
            for (int i = 0; i < _ds.Rows.Count; i++) _ds.Rows[i]["#"] = i + 1;
            lblDich.Text = $"Trạm trên tuyến {_maTuyen} ({_ds.Rows.Count})";
        }

        private void DaThayDoi(string thongBao)
        {
            _thayDoi = true;
            DanhSoLai();
            _xemTruoc?.Invoke(_ds);
            BaoThayDoi((thongBao == null ? "" : thongBao + " ") + "Chưa lưu.", false);
        }

        private void BaoThayDoi(string noiDung, bool canhBao)
        {
            lblThayDoi.Text = noiDung;
            lblThayDoi.ForeColor = canhBao ? Theme.WarningText : Theme.Muted;
        }

        // ------------------------------------------------------------------
        // Lưu
        // ------------------------------------------------------------------
        private async Task LuuAsync()
        {
            if (_ds.Rows.Count < 2)
            {
                UiHelper.CanhBao(this, "Một tuyến cần ít nhất 2 trạm (điểm đầu và điểm cuối).");
                return;
            }
            var ma = _ds.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["Mã"])).ToList();
            bool dungLai = chkDungLai.Checked;
            int noiThang = -1;
            if (!await UiHelper.ChayAsync(btnLuu, async () =>
                    noiThang = await Task.Run(() => TuyenRepository.LuuDanhSachTram(_maTuyen, ma, dungLai)),
                    dungLai ? "Đang lưu và dựng đường…" : "Đang lưu…"))
                return;

            _daLuu = true;
            _thayDoi = false;
            var thongBao = $"Đã lưu {ma.Count} trạm cho tuyến {_maTuyen}.";
            if (noiThang > 0)
                thongBao += $"\n{noiThang} đoạn giữa hai trạm không tìm được đường trên mạng đường nên được nối thẳng.";
            UiHelper.ThanhCong(this, thongBao);
            DaLuu?.Invoke();
            Close();
        }
    }
}
