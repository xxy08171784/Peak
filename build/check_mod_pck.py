# -*- coding: utf-8 -*-
# 解析 mod 的 pck（peak_new.pck），列出 localization 相关路径
import struct

pck_path = r'c:\Users\shiqian\Peak\bulid\peak_new.pck'
data = open(pck_path, 'rb').read()
print('total size', len(data))

pos = 4
version = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_major = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_minor = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_patch = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
pack_flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
file_base = struct.unpack('<Q', data[pos:pos+8])[0]; pos += 8
dir_offset = struct.unpack('<Q', data[pos:pos+8])[0]; pos += 8
print(f'version={version} godot {ver_major}.{ver_minor}.{ver_patch} flags={pack_flags}')
print(f'file_base={file_base} dir_offset={dir_offset}')

p = dir_offset
file_count = struct.unpack('<I', data[p:p+4])[0]; p += 4
print(f'file_count={file_count}')

loc_count = 0
for i in range(file_count):
    path_len = struct.unpack('<I', data[p:p+4])[0]; p += 4
    path = data[p:p+path_len].decode('utf-8', errors='replace').split('\x00')[0]; p += path_len
    ofs = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    size = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    p += 16  # md5
    flags = struct.unpack('<I', data[p:p+4])[0]; p += 4
    if 'localization' in path.lower() or 'scout' in path.lower() and 'top_panel' in path.lower():
        loc_count += 1
        if loc_count <= 40:
            print(f'  {path}  (size={size})')

print('total localization/scout_top_panel files:', loc_count)
