# -*- coding: utf-8 -*-
# 从 pck 提取 card_library.tscn 完整文本
pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 找 card_library.tscn 路径字符串出现的位置
target = b'res://scenes/screens/card_library/card_library.tscn'
positions = []
idx = 0
while True:
    idx = data.find(target, idx)
    if idx < 0:
        break
    positions.append(idx)
    idx += 1
print('found path string at:', positions)

# 场景文件一般以 [gd_scene 或 [gd_resource 开头，找离字符串最近的前方开头
for p in positions:
    # 向前搜索最近的 '[gd_scene' 或 '[gd_resource' 或 '[gd_node' 
    seg = data[max(0, p-300000):p]
    starts = []
    for marker in [b'[gd_scene', b'[gd_resource', b'[gd_node']:
        i = seg.rfind(marker)
        if i >= 0:
            starts.append(i + len(seg) - len(seg))  # 相对 seg 的 index
    print('candidate starts (rel to p):', starts)
    # 取最晚的开始位置
    if starts:
        s = p - 300000 + max(starts)
        # 向后找 ']' 结束 header
        # 直接提取 500KB
        e = min(len(data), s + 500000)
        content = data[s:e]
        print('--- extracted, starts with:', content[:120])
        print('--- ends with:', content[-120:])
        open(r'c:\Users\shiqian\Peak\bulid\extracted\card_library.tscn', 'wb').write(content)
        print('saved card_library.tscn')
