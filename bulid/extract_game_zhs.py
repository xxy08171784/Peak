# -*- coding: utf-8 -*-
import json, io

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# "铁甲战士" 在 1947032752，这是 zhs/cards.json 的中文内容
# 找这个 JSON 对象的起始 { 和结束 }
pos = data.find(b'"\xe9\x93\x81\xe7\x94\xb2\xe6\x88\x98\xe5\xa3\xab"')  # 铁甲战士
print('pos', pos)

# 向后找这个 JSON 的完整内容：向前找最近的 "{\n" 或 "{"
# 直接提取从该位置向后的 500KB 再解析
# 先看该位置周围
context = data[max(0,pos-500):pos+500]
# 找字符串起始
start = context.rfind(b'{')
print('relative start', start)
# 输出到文件
seg = data[max(0,pos-2000):pos+500000]
# 找 JSON 起始
json_start = seg.rfind(b'{')
print('seg json_start rel', json_start)
# 从 json_start 开始找平衡括号
s = json_start
depth = 0
in_str = False
esc = False
for i in range(s, len(seg)):
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

json_text = seg[s:end+1]
print('json length:', len(json_text))
try:
    obj = json.loads(json_text)
    print('PARSED OK, keys:', len(obj))
    print('sample:', list(obj.items())[:3])
except Exception as e:
    print('parse error:', e)
    # 保存以便检查
    io.open(r'C:\Users\shiqian\Peak\bulid\game_zhs_cards.json', 'w', encoding='utf-8').write(json_text)
    print('saved partial')
