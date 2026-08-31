# -*- coding: utf-8 -*-
# 从游戏 pck 提取 localization/zhs/cards.json 的 key 数量
import struct

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()
print('pck size:', len(data))

# 搜索 localization 路径
for needle in [b'localization/zhs/cards.json', b'localization/eng/cards.json']:
    pos = data.find(needle)
    print(needle.decode(), '->', pos)

# 找文件内容：从路径位置向后找 JSON 内容起始
# PCK 格式: 目录项(path, offset, size, md5, flags)，文件数据在末尾
# 直接搜索 "{\n  " 起始的 JSON 对象
import re
# 找 cards.json 的 JSON 内容：在文件名附近找 '{' 
pos = data.find(b'localization/zhs/cards.json')
if pos > 0:
    # 向前找文件数据偏移（目录项在文件开头，数据在末尾）
    # 简化：在文件末尾区域找 JSON 对象
    # 先看有没有其他 zhs 文件路径
    for needle in [b'localization/zhs/cards.json', b'localization/eng/cards.json', b'localization/zhs/powers.json']:
        p = data.find(needle)
        print(needle.decode(), 'at', p)
