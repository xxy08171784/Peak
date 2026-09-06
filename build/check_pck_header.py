# -*- coding: utf-8 -*-
# 检查游戏 pck 头部字节
import struct

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read(512)
print('first 32 bytes:', data[:32].hex())
print('ascii:', data[:16])

# Godot 4 标准: magic(4) format_ver(4) major(4) minor(4) patch(4) flags(4) count(4)
print('magic:', data[0:4])
print('format_ver:', struct.unpack('<I', data[4:8])[0])
print('major:', struct.unpack('<I', data[8:12])[0])
print('minor:', struct.unpack('<I', data[12:16])[0])
print('patch:', struct.unpack('<I', data[16:20])[0])
print('flags:', struct.unpack('<I', data[20:24])[0])
print('count:', struct.unpack('<I', data[24:28])[0])
