# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 涅奥 at 1947038857 —— 这是 zhs 内容
pos = data.find('涅奥'.encode('utf-8'))
print('涅奥 at', pos)

# 检查该位置前后的内容，确认是 ancients.json
print('context:', data[max(0,pos-200):pos+200])

# 找 zhs ancients.json 的 NEOW.epithet
# 从 pos 向前搜 "NEOW.epithet"
start = max(0, pos - 500000)
seg = data[start:pos+500000]
p2 = seg.find(b'"NEOW.epithet"')
print('NEOW.epithet rel', p2)
if p2 > 0:
    print('  ctx:', seg[p2:p2+200])
