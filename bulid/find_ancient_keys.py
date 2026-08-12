# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 找游戏内置 zhs/ancients.json：搜 NEOW.talk.IRONCLAD 的中文翻译（zhs）
# 先找 "NEOW" 相关 key 在 zhs 中
for needle in [b'"NEOW.talk.IRONCLAD.0-0.ancient"', b'NEOW.talk.IRONCLAD', b'"NEOW.epithet"', b'NEOW.epithet']:
    p = data.find(needle)
    print(needle.decode(), 'at', p)

# 找 zhs 的 ancients.json 内容：搜 "NEOW.talk.IRONCLAD.0-0.ancient" 后的中文
# 先找含 "NEOW.talk" 的字符串位置
positions = []
start = 0
while True:
    p = data.find(b'NEOW.talk', start)
    if p < 0:
        break
    positions.append(p)
    start = p + 1
print('total NEOW.talk positions:', len(positions))
for p in positions[:10]:
    print('  at', p, ':', data[p-30:p+80])
