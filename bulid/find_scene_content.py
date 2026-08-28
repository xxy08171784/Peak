# -*- coding: utf-8 -*-
# 找 [gd_scene ... card_library 的实际场景文件内容
pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 找所有 [gd_scene 开头，看哪些包含 card_library
import re
idx = 0
count = 0
while True:
    idx = data.find(b'[gd_scene', idx)
    if idx < 0:
        break
    # 检查前后是否有 card_library
    seg = data[max(0, idx-200): idx+800]
    if b'card_library' in seg or b'CardLibrary' in seg:
        count += 1
        print(f'=== gd_scene @{idx}:')
        print(seg[:500].decode('utf-8', errors='replace'))
        print('...')
    idx += 1
print('total scenes containing card_library:', count)
