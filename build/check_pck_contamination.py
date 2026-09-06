# -*- coding: utf-8 -*-
# 检查 mod PCK 中是否包含 bulid/extracted 下的资源（这些是误打包的提取文件）
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

extracted = 0
scenes_override = 0
for i in range(file_count):
    path_len = struct.unpack('<I', data[p:p+4])[0]; p += 4
    path = data[p:p+path_len].decode('utf-8', errors='replace').split('\x00')[0]; p += path_len
    ofs = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    size = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    p += 16
    flags = struct.unpack('<I', data[p:p+4])[0]; p += 4
    if 'bulid/extracted' in path or 'extracted' in path.lower():
        extracted += 1
        if extracted <= 20:
            print('EXTRACTED:', path, 'size', size)
    if path.startswith('scenes/') and (path.endswith('.tscn') or path.endswith('.tscn.remap')):
        scenes_override += 1
        print('SCENE:', path, 'size', size)

print()
print('extracted files in pck:', extracted)
print('scene files in pck:', scenes_override)
