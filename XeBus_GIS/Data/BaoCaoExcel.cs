using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace XeBus_GIS.Data
{
    /// <summary>Quy trình 4.4 – Xuất file báo cáo thống kê (Excel .xlsx, ClosedXML).</summary>
    public static class BaoCaoExcel
    {
        private static readonly XLColor MauTieuDe = XLColor.FromHtml("#0F172A");
        private static readonly XLColor MauNhan = XLColor.FromHtml("#475569");

        /// <param name="tongQuan">cặp (chỉ tiêu, giá trị) cho trang Tổng quan</param>
        /// <param name="cacBang">tên sheet -> bảng dữ liệu (các cột "ma", "wkt" bị bỏ qua)</param>
        /// <param name="anhBanDo">đường dẫn ảnh PNG bản đồ (có thể null)</param>
        public static void Xuat(string duongDan, string tieuDe, string nguoiLap,
                                IList<KeyValuePair<string, string>> tongQuan,
                                IList<KeyValuePair<string, DataTable>> cacBang,
                                string anhBanDo)
        {
            using (var wb = new XLWorkbook())
            {
                // ---- Trang tổng quan ----
                var ws = wb.Worksheets.Add("Tổng quan");
                ws.Cell(1, 1).Value = tieuDe;
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Font.FontColor = MauTieuDe;
                ws.Cell(2, 1).Value = "Hệ thống GIS hỗ trợ quản lý, phân tích, thiết lập trạm xe buýt – TP Hà Nội";
                ws.Cell(2, 1).Style.Font.FontColor = MauNhan;
                ws.Cell(3, 1).Value = $"Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm}   ·   Người lập: {nguoiLap}";
                ws.Cell(3, 1).Style.Font.FontColor = MauNhan;

                int dong = 5;
                ws.Cell(dong, 1).Value = "Chỉ tiêu";
                ws.Cell(dong, 2).Value = "Giá trị";
                var hdr = ws.Range(dong, 1, dong, 2);
                hdr.Style.Font.Bold = true;
                hdr.Style.Font.FontColor = XLColor.White;
                hdr.Style.Fill.BackgroundColor = MauTieuDe;
                foreach (var kv in tongQuan)
                {
                    dong++;
                    ws.Cell(dong, 1).Value = kv.Key;
                    ws.Cell(dong, 2).Value = kv.Value;
                    ws.Cell(dong, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
                ws.Range(5, 1, dong, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(5, 1, dong, 2).Style.Border.InsideBorder = XLBorderStyleValues.Hair;
                ws.Column(1).Width = 46;
                ws.Column(2).Width = 22;

                dong += 2;
                ws.Cell(dong, 1).Value = "Ghi chú: dân số khu dân cư là ước tính (phân bổ dân số phường theo diện tích khu ở OSM); " +
                                         "dân số phường/xã là số liệu thống kê.";
                ws.Cell(dong, 1).Style.Font.Italic = true;
                ws.Cell(dong, 1).Style.Font.FontColor = MauNhan;

                if (!string.IsNullOrEmpty(anhBanDo) && File.Exists(anhBanDo))
                {
                    ws.Cell(dong + 2, 1).Value = "Bản đồ tại thời điểm xuất báo cáo";
                    ws.Cell(dong + 2, 1).Style.Font.Bold = true;
                    var pic = ws.AddPicture(anhBanDo).MoveTo(ws.Cell(dong + 3, 1));
                    if (pic.Width > 900) pic.ScaleWidth(900.0 / pic.Width, true);
                }

                // ---- Các bảng chi tiết ----
                foreach (var kv in cacBang)
                {
                    var bang = kv.Value.Copy();
                    foreach (var cot in new[] { "ma", "wkt", "kinhdo", "vido" })
                        if (bang.Columns.Contains(cot)) bang.Columns.Remove(cot);
                    var ten = TenSheetHopLe(kv.Key, wb);
                    var s = wb.Worksheets.Add(ten);
                    s.Cell(1, 1).Value = kv.Key;
                    s.Cell(1, 1).Style.Font.Bold = true;
                    s.Cell(1, 1).Style.Font.FontSize = 13;
                    s.Cell(1, 1).Style.Font.FontColor = MauTieuDe;
                    if (bang.Rows.Count == 0)
                    {
                        s.Cell(3, 1).Value = "(không có dữ liệu)";
                        continue;
                    }
                    var t = s.Cell(3, 1).InsertTable(bang, true);
                    t.Theme = XLTableTheme.TableStyleLight9;
                    t.ShowAutoFilter = true;
                    s.SheetView.FreezeRows(3);
                    s.Columns().AdjustToContents(3, Math.Min(bang.Rows.Count + 3, 200));
                    foreach (var c in s.ColumnsUsed()) if (c.Width > 60) c.Width = 60;
                }

                wb.SaveAs(duongDan);
            }
        }

        /// <summary>Tên sheet Excel: tối đa 31 ký tự, không chứa : \ / ? * [ ], không trùng.</summary>
        private static string TenSheetHopLe(string ten, XLWorkbook wb)
        {
            var sach = new string(ten.Select(c => ":\\/?*[]".IndexOf(c) >= 0 ? '-' : c).ToArray());
            if (sach.Length > 31) sach = sach.Substring(0, 31);
            var goc = sach;
            int i = 2;
            while (wb.Worksheets.Any(w => w.Name.Equals(sach, StringComparison.OrdinalIgnoreCase)))
                sach = (goc.Length > 28 ? goc.Substring(0, 28) : goc) + " " + i++;
            return sach;
        }
    }
}
