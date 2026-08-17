# -*- coding: utf-8 -*-
# 在 pck 中搜索 card_library 相关字符串
pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

for kw in [b'card_library', b'CardLibrary', b'cardlibrary', b'CardLibraryScreen', b'NCardLibrary']:
    idx = 0
    found = []
    while True:
        idx = data.find(kw, idx)
        if idx < 0:
            break
        s = max(0, idx-100)
        e = min(len(data), idx+150)
        chunk = data[s:e]
        try:
            txt = chunk.decode('utf-8', errors='replace')
        except:
            txt = ''
        found.append((idx, txt))
        idx += 1
    print(f'=== pck kw={kw} found={len(found)}')
    for i, t in found[:10]:
        print(f'  @{i}: ...{t.replace(chr(0),"|")}...')
