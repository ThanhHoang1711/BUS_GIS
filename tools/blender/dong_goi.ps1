# Gộp chuỗi khung JPEG do dung_canh.py render thành tệp nền trang đăng nhập (đọc bởi UI/Controls/NenVideo.cs).
# Định dạng: "XBV1" · int32 fps · int32 số khung · ([int32 độ dài] [byte JPEG])…
#   powershell -File tools/blender/dong_goi.ps1 -ThuMuc <thư mục k_*.jpg> -Ra XeBus_GIS/Assets/nen_dangnhap.bin [-Fps 24] [-ChatLuong 60] [-Rong 0]
param(
    [Parameter(Mandatory)] [string]$ThuMuc,
    [Parameter(Mandatory)] [string]$Ra,
    [int]$Fps = 24,
    [int]$ChatLuong = 60,   # chất lượng JPEG khi nén lại (ảnh nền được phủ tối nên 55–65 là đủ)
    [int]$Rong = 0          # >0: thu nhỏ về chiều rộng này
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$codec = [Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$ts = New-Object Drawing.Imaging.EncoderParameters 1
$ts.Param[0] = New-Object Drawing.Imaging.EncoderParameter ([Drawing.Imaging.Encoder]::Quality), ([long]$ChatLuong)

$tep = Get-ChildItem $ThuMuc -Filter 'k_*.jpg' | Sort-Object Name
if ($tep.Count -eq 0) { throw "Không có khung k_*.jpg trong $ThuMuc" }
$null = New-Item -ItemType Directory -Force (Split-Path $Ra)
$fs = [IO.File]::Create($Ra)
$bw = New-Object IO.BinaryWriter $fs
$bw.Write([Text.Encoding]::ASCII.GetBytes('XBV1'))
$bw.Write([int]$Fps)
$bw.Write([int]$tep.Count)
foreach ($t in $tep) {
    $anh = [Drawing.Image]::FromFile($t.FullName)
    if ($Rong -gt 0 -and $anh.Width -gt $Rong) {
        $cao = [int]($anh.Height * $Rong / $anh.Width)
        $nho = New-Object Drawing.Bitmap $Rong, $cao
        $g = [Drawing.Graphics]::FromImage($nho)
        $g.InterpolationMode = 'HighQualityBicubic'
        $g.DrawImage($anh, 0, 0, $Rong, $cao)
        $g.Dispose(); $anh.Dispose(); $anh = $nho
    }
    $ms = New-Object IO.MemoryStream
    $anh.Save($ms, $codec, $ts)
    $anh.Dispose()
    $b = $ms.ToArray()
    $bw.Write([int]$b.Length)
    $bw.Write($b)
}
$bw.Close()
"{0} khung, {1:N1} MB -> {2}" -f $tep.Count, ((Get-Item $Ra).Length / 1MB), $Ra
