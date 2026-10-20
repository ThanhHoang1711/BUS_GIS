"""
Tải tài nguyên CC0 (Poly Haven – https://polyhaven.com, giấy phép CC0) dùng cho cảnh Blender vào tools/blender/assets/
(thư mục này nằm trong .gitignore – chạy lại script khi clone mới).

    python tools/blender/tai_tai_nguyen.py

Đã có file đúng kích thước thì bỏ qua (chạy lại an toàn).
"""
import json
import os
import sys
import urllib.request

GOC = os.path.join(os.path.dirname(os.path.abspath(__file__)), "assets")
API = "https://api.polyhaven.com/files/"
UA = {"User-Agent": "XeBus_GIS-blender/1.0"}

# (id, loại, độ phân giải)
DANH_SACH = [
    ("kloofendal_48d_partly_cloudy_puresky", "hdri", "4k"),   # trời xanh, mây rải rác (dùng chính)
    ("citrus_orchard_puresky", "hdri", "4k"),     # trời chiều, mây rải rác (dự phòng)
    ("jacaranda_tree", "model", "1k"),           # cây tán lớn hoa tím (giống bằng lăng) – quanh hồ, công viên
    ("tree_small_02", "model", "1k"),            # cây vỉa hè
    ("street_lamp_01", "model", "1k"),           # đèn đường
    ("asphalt_02", "texture", "2k"),             # mặt đường
    ("concrete_tiles", "texture", "2k"),         # gạch lát vỉa hè
    ("concrete_floor_01", "texture", "2k"),      # nền bê tông / sân
]
MAP_TEXTURE = ["Diffuse", "nor_gl", "Rough"]


def json_url(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=UA), timeout=60) as r:
        return json.load(r)


def tai(url, dich, kich_thuoc=None):
    if os.path.exists(dich) and (kich_thuoc is None or os.path.getsize(dich) == kich_thuoc):
        return False
    os.makedirs(os.path.dirname(dich), exist_ok=True)
    tam = dich + ".part"
    with urllib.request.urlopen(urllib.request.Request(url, headers=UA), timeout=300) as r, open(tam, "wb") as f:
        while True:
            khoi = r.read(1 << 20)
            if not khoi:
                break
            f.write(khoi)
    os.replace(tam, dich)
    return True


def main():
    tong = 0
    for ma, loai, dpg in DANH_SACH:
        tt = json_url(API + ma)
        thu_muc = os.path.join(GOC, ma)
        if loai == "hdri":
            f = tt["hdri"][dpg]["hdr"]
            dich = os.path.join(thu_muc, ma + ".hdr")
            moi = tai(f["url"], dich, f["size"])
            tong += f["size"]
        elif loai == "model":
            f = tt["blend"][dpg]["blend"]
            dich = os.path.join(thu_muc, ma + ".blend")
            moi = tai(f["url"], dich, f["size"])
            tong += f["size"]
            for duong_dan, inc in (f.get("include") or {}).items():
                tai(inc["url"], os.path.join(thu_muc, duong_dan.replace("/", os.sep)), inc["size"])
                tong += inc["size"]
        else:
            moi = False
            for m in MAP_TEXTURE:
                f = tt[m][dpg]["jpg"] if "jpg" in tt[m][dpg] else tt[m][dpg]["png"]
                ext = ".jpg" if "jpg" in tt[m][dpg] else ".png"
                moi |= tai(f["url"], os.path.join(thu_muc, "%s_%s%s" % (ma, m, ext)), f["size"])
                tong += f["size"]
        print(("tải  " if moi else "có   ") + ma)
    print("xong – %.0f MB trong %s" % (tong / 1e6, GOC))


if __name__ == "__main__":
    sys.exit(main())
