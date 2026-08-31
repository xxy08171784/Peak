# -*- coding: utf-8 -*-
# 在游戏 exe 和 pck 中搜索 card_library 资源路径
import struct

exe_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe'
pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'

# 搜索 exe 中的 res:// 路径字符串
data = open(exe_path, 'rb').read()
print('exe size', len(data))
for kw in [b'card_library', b'cardlibrary', b'CardLibrary']:
    idx = 0
    found = []
    while True:
        idx = data.find(kw, idx)
        if idx < 0:
            break
        # 打印周围字符串
        s = max(0, idx-80)
        e = min(len(data), idx+120)
        chunk = data[s:e]
        try:
            txt = chunk.decode('utf-8', errors='replace')
        except:
            txt = ''
        found.append(txt)
        idx += 1
    print(f'=== exe kw={kw} found={len(found)}')
    for t in found[:8]:
        print('  ...', t.replace('\x00', '|'))
