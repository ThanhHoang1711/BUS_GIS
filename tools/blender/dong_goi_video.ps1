# Ghép chuỗi khung JPEG do dung_canh.py render (k_0001…k_0600) thành video nền trang đăng nhập lặp liền + ảnh tĩnh.
#   powershell -File tools/blender/dong_goi_video.ps1 -ThuMuc <thư mục k_*.jpg> -Ra XeBus_GIS/Assets/nen_dangnhap.mp4
# Vòng lặp: render dư 24 khung (577–600); khung ra k (1…24) = hoà render[576+k] (mờ dần) với render[k] (rõ dần)
# ⇒ khung cuối (576) nối tiếp tự nhiên sang khung đầu, không lộ điểm lặp.
# Cần ffmpeg (winget install Gyan.FFmpeg). H.264 High yuv420p +faststart ⇒ Windows Media Foundation (WPF MediaElement) phát được.
param(
    [Parameter(Mandatory)] [string]$ThuMuc,
    [Parameter(Mandatory)] [string]$Ra,
    [int]$Fps = 24,
    [int]$Khung = 576,     # độ dài vòng lặp (khung)
    [int]$Hoa = 24,        # số khung hoà ở điểm lặp
    [int]$Crf = 20,        # chất lượng x264: 18 rất nét … 23 nhẹ hơn
    [int]$KhungAnh = 200   # khung làm ảnh tĩnh (xe buýt đang đỗ ở trạm)
)
$ErrorActionPreference = 'Stop'
$ff = (Get-Command ffmpeg -ErrorAction Stop).Source
$null = New-Item -ItemType Directory -Force (Split-Path $Ra)
$tong = $Khung + $Hoa
$thieu = 1..$tong | Where-Object { -not (Test-Path (Join-Path $ThuMuc ('k_{0:D4}.jpg' -f $_))) -or (Get-Item (Join-Path $ThuMuc ('k_{0:D4}.jpg' -f $_))).Length -eq 0 }
if ($thieu) { throw "Thiếu $($thieu.Count) khung (vd k_$('{0:D4}' -f $thieu[0]).jpg) – render tiếp rồi chạy lại." }

$loc = "[0:v]split=3[a][b][c];" +
       "[a]trim=start_frame=0:end_frame=$Hoa,setpts=PTS-STARTPTS[dau];" +
       "[b]trim=start_frame=$($Khung):end_frame=$tong,setpts=PTS-STARTPTS[duoi];" +
       "[dau][duoi]blend=all_expr='A*(N+1)/$Hoa+B*(1-(N+1)/$Hoa)'[hoa];" +
       "[c]trim=start_frame=$($Hoa):end_frame=$($Khung),setpts=PTS-STARTPTS[giua];" +
       "[hoa][giua]concat=n=2:v=1:a=0,scale=out_range=tv,format=yuv420p[ra]"
& $ff -hide_banner -loglevel error -y -framerate $Fps -start_number 1 -i (Join-Path $ThuMuc 'k_%04d.jpg') `
    -filter_complex $loc -map "[ra]" -c:v libx264 -preset slow -crf $Crf -pix_fmt yuv420p -profile:v high -level 4.1 `
    -movflags +faststart -an $Ra
if ($LASTEXITCODE -ne 0) { throw "ffmpeg lỗi $LASTEXITCODE" }

# ảnh tĩnh: khung có xe buýt đỗ ở trạm (hiện khi chưa mở được video / người dùng tắt hiệu ứng)
$anh = [IO.Path]::ChangeExtension($Ra, '.jpg')
& $ff -hide_banner -loglevel error -y -i (Join-Path $ThuMuc ('k_{0:D4}.jpg' -f $KhungAnh)) -q:v 4 $anh
"video {0:N1} MB ({1} khung), ảnh tĩnh {2:N0} KB -> {3}" -f ((Get-Item $Ra).Length / 1MB), $Khung, ((Get-Item $anh).Length / 1KB), $Ra
