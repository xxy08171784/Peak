# -*- coding: utf-8 -*-
# 从 pck 提取 card_library.tscn 和 library_pool_toggle.tscn 完整文本
pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

import os
os.makedirs(r'c:\Users\shiqian\Peak\bulid\extracted', exist_ok=True)

# card_library.tscn @1978329136
# library_pool_toggle.tscn @1978365600
targets = [
    (1978329136, 'card_library.tscn'),
    (1978365600, 'library_pool_toggle.tscn'),
]

for start, name in targets:
    # 场景文件以 [gd_scene 开头。向后找下一个 [gd_scene 或下一个文件边界
    # 找下一个 [gd_scene
    nxt = data.find(b'[gd_scene', start + 10)
    # 也找下一个 "res://" 资源路径字符串作为边界
    nxt2 = data.find(b'res://', start + 10)
    end_candidates = [x for x in [nxt, nxt2] if x > start]
    end = min(end_candidates) if end_candidates else start + 300000
    content = data[start:end]
    print(f'--- {name}: start={start} end={end} size={len(content)}')
    print(content[:300].decode('utf-8', errors='replace'))
    print('...TAIL...')
    print(content[-200:].decode('utf-8', errors='replace'))
    with open(rf'c:\Users\shiqian\Peak\bulid\extracted\{name}', 'wb') as f:
        f.write(content)
    print('saved', name)
