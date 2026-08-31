# -*- coding: utf-8 -*-
import json, io, re

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 游戏内置 zhs/cards.json 在路径 1965906068 附近。文件内容应该在 pck 文件数据区
# Godot pck 的目录在开头，数据在末尾。搜索数据区中的 JSON。
# 找 "IRONCLAD" 或游戏内置卡 key 在 zhs 翻译（中文值）中出现的位置
# 先找 "打击"（Strike 的中文翻译）
for needle in ['"打击"'.encode('utf-8'), '"铁甲战士"'.encode('utf-8'), '"STRENGTH"'.encode('utf-8')]:
    p = data.find(needle)
    print(needle.decode('utf-8', errors='replace'), 'at', p)

# 找游戏内置 zhs cards.json 的 key（如 STRIKE_IRONCLAD 的中文 title）
# 游戏内置卡是 IRONCLAD 等，zhs 有翻译。搜 "打击" 中文值
# 也搜 eng 的 STRIKE_IRONCLAD.title = "Strike"
p = data.find(b'"STRIKE_IRONCLAD.title": "Strike"')
print('eng strike at', p)

# 搜索 {"\n  " 结构（JSON 对象开始）
for needle in [b'{\n  "STRIKE_IRONCLAD', b'{\n\t"STRIKE_IRONCLAD']:
    p = data.find(needle)
    print(needle.decode(), 'at', p)
