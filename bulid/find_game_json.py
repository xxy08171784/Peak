# -*- coding: utf-8 -*-
import json, io

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 从路径位置向后搜索 JSON 对象 { 起始
# 文件数据在 pck 末尾区域，路径在目录区。文件内容在 offset 处
# 直接搜索 zhs/cards.json 内容特征
pos = data.find(b'localization/zhs/cards.json')
print('path at', pos)

# 文件内容应该在路径附近的 offset。搜索 JSON 对象起始
# 简化：在整个文件中找 "cards.json" 的 JSON 内容 —— 找 {"\n  " 后跟常见key
# 找 "IRONCLAD" 或常见卡 key 出现的位置
for needle in [b'"STRIKE_IRONCLAD.title"', b'"IRONCLAD.title"', b'"STRIKE_SCOUT.title"', b'"SPORE_CLOUD.title"']:
    p = data.find(needle)
    print(needle.decode(), 'at', p)
