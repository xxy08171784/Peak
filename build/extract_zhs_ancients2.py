# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 涅奥位置
pos = data.find('涅奥'.encode('utf-8'))
print('涅奥 at', pos)

# 向前找最近的文件边界：ancients.json 的开头应该在某个 '{"\n' 处
# 搜索该文件内的第一个 key。游戏 zhs ancients.json 以 "NEOW.epithet" 或 "ARCHITECT.epithet" 开始
# 从 pos 向前 3MB 内找这些模式
region_start = max(0, pos - 3000000)
region = data[region_start:pos]

# 找 "epithet" 的所有位置
epithet_positions = [m.start() for m in re.finditer(rb'"(\w+)\.epithet"', region)]
print('epithet in region:', len(epithet_positions))

# 找第一个 epithet 前面最近的 "{\n  \"" 
if epithet_positions:
    first_ep = region_start + epithet_positions[0]
    print('first epithet at', first_ep, ':', data[first_ep-50:first_ep+80])
    # 向前找 "{"
    before = data[max(0, first_ep-3000):first_ep]
    brace = before.rfind(b'{')
    if brace >= 0:
        start = max(0, first_ep-3000) + brace
        print('json start at', start, ':', data[start:start+120])
        # 平衡括号找结束
        seg = data[start:start+1000000]
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
            try:
                obj = json.loads(json_bytes)
                print('PARSED OK keys:', len(obj))
                from collections import Counter
                prefixes = Counter(k.split('.')[0] for k in obj)
                print('ancient prefixes:', dict(prefixes))
            except Exception as e:
                print('parse error:', e)
