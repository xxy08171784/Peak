# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 找所有 DARV.epithet 位置，找后面是中文的那个
pos_list = []
start = 0
while True:
    p = data.find(b'"DARV.epithet"', start)
    if p < 0:
        break
    pos_list.append(p)
    start = p + 1
print('DARV.epithet positions:', len(pos_list))
for p in pos_list:
    chunk = data[p:p+60]
    print('  at', p, ':', chunk)

# 找 zhs 的（中文值）
for p in pos_list:
    chunk = data[p:p+120]
    if re.search(rb'[\xe4-\xe9][\x80-\xbf][\x80-\xbf]', chunk):
        print('ZHS at', p, ':', chunk)
        zhs_pos = p
        break
