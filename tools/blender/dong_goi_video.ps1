# Ghép chuỗi khung JPEG do dung_canh.py render thành video nền trang đăng nhập (phát bởi UI/Controls/NenVideo.cs
# bằng WPF MediaElement) + ảnh tĩnh cùng tên (khung đầu – hiện khi chưa mở được video / tắt hiệu ứng).
#   powershell -File tools/blender/dong_goi_video.ps1 -ThuMuc <thư mục k_*.jpg> -Ra XeBus_GIS/Assets/nen_dangnhap.mp4
# Cần ffmpeg (winget install Gyan.FFmpeg). H.264 yuv420p (+faststart) để Windows Media Foundation phát được ở mọi máy.
param(
    [Parameter(Mandatory)] [string]$ThuMuc,
    [Parameter(Mandatory)] [string]$Ra,
    [int]$Fps = 24,
    [int]$Crf = 20        # chất lượng x264: 18 rất nét … 23 nhẹ hơn
)
$ErrorActionPreference = 'Stop'
$ff = (Get-Command ffmpeg -ErrorAction Stop).Source
$null = New-Item -ItemType Directory -Force (Split-Path $Ra)
& $ff -hide_banner -loglevel error -y -framerate $Fps -i (Join-Path $ThuMuc 'k_%04d.jpg') `
    -vf "scale=out_range=tv,format=yuv420p" -c:v libx264 -preset slow -crf $Crf -pix_fmt yuv420p -profile:v high -level 4.1 `
    -movflags +faststart -an $Ra
if ($LASTEXITCODE -ne 0) { throw "ffmpeg lỗi $LASTEXITCODE" }

# ảnh tĩnh: khung đầu, nén lại JPEG 85
Add-Type -AssemblyName System.Drawing
$codec = [Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$ts = New-Object Drawing.Imaging.EncoderParameters 1
$ts.Param[0] = New-Object Drawing.Imaging.EncoderParameter ([Drawing.Imaging.Encoder]::Quality), ([long]85)
$anh = [Drawing.Image]::FromFile((Join-Path $ThuMuc 'k_0001.jpg'))
$anh.Save([IO.Path]::ChangeExtension($Ra, '.jpg'), $codec, $ts)
$anh.Dispose()
"video {0:N1} MB, ảnh tĩnh {1:N0} KB -> {2}" -f ((Get-Item $Ra).Length / 1MB), ((Get-Item ([IO.Path]::ChangeExtension($Ra, '.jpg'))).Length / 1KB), $Ra
