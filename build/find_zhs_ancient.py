# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 游戏内置 zhs ancients.json：搜 NEOW.talk.IRONCLAD 中文翻译
# 中文字符特征
targets = [
    '"NEOW.epithet": "'.encode('utf-8'),
    '"NEOW.title": "'.encode('utf-8'),
    'NEOW.talk.IRONCLAD.0-0.ancient": "'.encode('utf-8'),
    '"NEOW.talk.SCOUT'.encode('utf-8'),
]
for t in targets:
    p = data.find(t)
    print(t.decode('utf-8', errors='replace'), 'at', p)
    if p > 0:
        print('   ctx:', data[p:p+100])

# 找含 "NEOW.talk" 且后面跟中文字符的位置（zhs 区域）
# zhs 应该在 pck 的 localization/zhs 目录，路径在 1965M 附近
# 文件内容：搜索 1940M-1965M 之间的 NEOW.talk
start, end = 1935000000, 1965000000
positions = []
s = start
while True:
    p = data.find(b'NEOW.talk', s)
    if p < 0 or p > end:
        break
    positions.append(p)
    s = p + 1
print('\nNEOW.talk in zhs region:', len(positions))
for p in positions[:5]:
    print('  at', p, ':', data[p-20:p+120])
