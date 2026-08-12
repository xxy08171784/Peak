# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 找中文字符的 NEOW 对话：搜 "NEOW.talk.ANY.0-0r.ancient" 后跟中文字符
# 先找所有含 "NEOW.talk" 的位置，然后检查后面是不是中文
import re
# 中文字符 UTF-8 范围 \xe4\xb8\x80-\xe9\xbe\xa5
positions = []
start = 0
while True:
    p = data.find(b'NEOW.talk', start)
    if p < 0:
        break
    # 检查 p 后面 200 字节内是否有中文字符
    chunk = data[p:p+300]
    if re.search(rb'[\xe4-\xe9][\x80-\xbf][\x80-\xbf]', chunk):
        positions.append(p)
    start = p + 1

print('zhs NEOW.talk positions:', len(positions))
for p in positions[:5]:
    print('  at', p, ':', data[p:p+150])

# 找 NEOW.epithet 中文
for needle in ['Mutter der Auferweckung'.encode('utf-8'), '"\xe4\xb9\x83\xe4\xb9\x8c"'.encode('utf-8')]:
    pass

# 直接搜中文字符 "欢迎" 或 "高塔" 附近的 NEOW
for w in ['欢迎来到高塔'.encode('utf-8'), '复活之母'.encode('utf-8'), '尼奥'.encode('utf-8'), '涅奥'.encode('utf-8'), '高塔'.encode('utf-8')]:
    p = data.find(w)
    print(w.decode('utf-8'), 'at', p)
