# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# zhs 从 1947035364（囤积者）
pos = data.find('"DARV.epithet": "\u56e4\u79ef\u8005"'.encode('utf-8'))
print('zhs DARV.epithet at', pos)
if pos < 0:
    # 囤积者
    pos = data.find('\u56e4\u79ef\u8005'.encode('utf-8'))
    print('fallback at', pos)

# 向前找文件起始 "{\n"
before = data[max(0, pos-3000):pos]
brace = before.rfind(b'{')
start = max(0, pos-3000) + brace
print('json start at', start, ':', data[start:start+100])

# 平衡括号找结束
seg = data[start:start+2000000]
depth = 0; in_str = False; esc = False; end = -1
for i in range(len(seg)):
    c = seg[i:i+1]
    if in_str:
        if esc: esc = False
        elif c == b'\\': esc = True
        elif c == b'"': in_str = False
    else:
        if c == b'"': in_str = True
        elif c == b'{': depth += 1
        elif c == b'}':
            depth -= 1
            if depth == 0:
                end = i; break

if end > 0:
    json_bytes = seg[:end+1]
    io.open(r'C:\Users\shiqian\Peak\bulid\game_zhs_ancients.json', 'wb').write(json_bytes)
    print('saved size', len(json_bytes))
    obj = json.loads(json_bytes)
    print('PARSED OK keys:', len(obj))
    from collections import Counter
    prefixes = Counter(k.split('.')[0] for k in obj)
    print('prefixes:', dict(prefixes))
    # 打印 NEOW 相关 key
    print('\nNEOW keys:')
    for k in sorted(obj):
        if k.startswith('NEOW') and ('epithet' in k or 'title' in k or 'talk.ANY' in k):
            print(' ', k, '=', obj[k][:60])
else:
    print('no end found')
