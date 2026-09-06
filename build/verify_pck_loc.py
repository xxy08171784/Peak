# -*- coding: utf-8 -*-
# 验证 peak_new.pck 中 card_library.json 是否存在
import struct

pck_path = r'c:\Users\shiqian\Peak\bulid\peak_new.pck'
data = open(pck_path, 'rb').read()

pos = 4
version = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_major = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_minor = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_patch = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
pack_flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
file_base = struct.unpack('<Q', data[pos:pos+8])[0]; pos += 8
dir_offset = struct.unpack('<Q', data[pos:pos+8])[0]; pos += 8

p = dir_offset
file_count = struct.unpack('<I', data[p:p+4])[0]; p += 4

target = 'localization/zhs/card_library.json'
found = False
for i in range(file_count):
    path_len = struct.unpack('<I', data[p:p+4])[0]; p += 4
    path = data[p:p+path_len].decode('utf-8', errors='replace').split('\x00')[0]; p += path_len
    ofs = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    size = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    p += 16
    flags = struct.unpack('<I', data[p:p+4])[0]; p += 4
    if path == target:
        found = True
        content = data[file_base + ofs: file_base + ofs + size]
        print('FOUND:', path, 'size', size)
        # 检查 POOL_SCOUT_TIP
        txt = content.decode('utf-8', errors='replace')
        print('has POOL_SCOUT_TIP:', 'POOL_SCOUT_TIP' in txt)
        print('has CARD_COUNT:', '"CARD_COUNT"' in txt)
        break

print('found:', found)
