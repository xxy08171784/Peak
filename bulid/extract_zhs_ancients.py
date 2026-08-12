# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 游戏内置 zhs/ancients.json：从"涅奥"（1947038857）向前找文件起始
# 找 ancients.json 的内容起点：在 1941341967（ja 的 NEOW.talk）到 1947038857 之间
# 更精确：找 zhs ancients.json 的开头 "{" 后跟 "NEOW.epithet" 或 "DARV.epithet"
# 找 '"NEOW.epithet": "' 的中文版位置
target = b'"NEOW.epithet": "\xe5\xa4\x8d\xe7\x94\x9f\xe4\xb9\x8b\xe6\xaf\x8d'  # 复活之母
pos = data.find(target)
print('NEOW.epithet zhs at', pos)

# 从这个位置向前找 JSON 对象起始（"{\n" 前最近的 '{'）
# zhs ancients.json 前面可能有别的语言包，找本文件的开头
# 找 "NEOW.title" 中文版
t2 = b'"NEOW.title": "\xe6\xb6\x85\xe5\xa5\xa5"'  # 涅奥
p2 = data.find(t2)
print('NEOW.title zhs at', p2)

# 找文件开头：从 pos 向前 2MB 内找 "\n{\n  \"N" 或类似
region = data[max(0, pos-2000000):pos]
# 找最后一个文件边界：搜 '\x00\x00' 或明显的分隔
# 直接找 "epithet" 前的第一个 '"' 开头的 JSON 起始
m = None
for mm in re.finditer(rb'\n\{\n  "N(?:EOW|ONU|DARV|TEZ|ORO|PAEL|VAK|TAN|ARCH)', region):
    m = mm
if m:
    abs_start = max(0, pos-2000000) + m.start()
    print('json start at', abs_start)
    # 找结束：从 abs_start 平衡括号
    seg = data[abs_start:abs_start+800000]
    depth = 0
    in_str = False
    esc = False
    end = -1
    for i in range(len(seg)):
        c = seg[i:i+1]
        if in_str:
            if esc:
                esc = False
            elif c == b'\\':
                esc = True
            elif c == b'"':
                in_str = False
        else:
            if c == b'"':
                in_str = True
            elif c == b'{':
                depth += 1
            elif c == b'}':
                depth -= 1
                if depth == 0:
                    end = i
                    break
    print('json end at rel', end)
    if end > 0:
        json_bytes = seg[:end+1]
        io.open(r'C:\Users\shiqian\Peak\bulid\game_zhs_ancients.json', 'wb').write(json_bytes)
        print('saved, size', len(json_bytes))
        try:
            obj = json.loads(json_bytes)
            print('PARSED OK, keys:', len(obj))
            # 打印所有顶层 key 前缀统计
            from collections import Counter
            prefixes = Counter(k.split('.')[0] for k in obj.keys())
            print('prefix counts:', dict(prefixes))
        except Exception as e:
            print('parse error:', e)
else:
    print('no start found')
