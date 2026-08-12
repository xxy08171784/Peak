# -*- coding: utf-8 -*-
import json, io

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 游戏内置 zhs/cards.json 内容在 1947M 附近（铁甲战士）
# 找 "铁甲战士" 在 cards.json 值中的位置
target = '"铁甲战士"'.encode('utf-8')
pos = data.find(target)
print('target at', pos)

# 找这个值对应的 key。向该位置前 1000 字节内找 "\"XXX.title\":" 
# 直接找 JSON 对象起点：向前找最近的 "{\n" 或 "{" 后面跟引号
# 用正则找 JSON 对象起始
import re
# 在 pos 前 10MB 内找文件开头（可能前面有 header）
region_start = max(0, pos - 2000000)
region = data[region_start:pos+100]

# 找最后一个 "\n{\n  \"" 结构（JSON 对象开始）
m = None
for mm in re.finditer(rb'\{\s*"(STRIKE_IRONCLAD|IRONCLAD|STRIKE_SILENT|BASIC)', data[max(0,pos-3000000):pos]):
    m = mm

print('found obj start at rel', m.start() if m else None)
