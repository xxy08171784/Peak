# -*- coding: utf-8 -*-
# 列出 mod PCK 中所有文件路径，找出污染源
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

paths = []
for i in range(file_count):
    path_len = struct.unpack('<I', data[p:p+4])[0]; p += 4
    path = data[p:p+path_len].decode('utf-8', errors='replace').split('\x00')[0]; p += path_len
    ofs = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    size = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    p += 16
    flags = struct.unpack('<I', data[p:p+4])[0]; p += 4
    paths.append((path, size))

print('total files:', len(paths))
print()
print('=== .godot/ files ===')
for path, size in paths:
    if path.startswith('.godot/'):
        print(f'  {path} ({size})')

print()
print('=== bulid/ files (should be none) ===')
for path, size in paths:
    if 'bulid' in path:
        print(f'  {path} ({size})')

print()
print('=== 1.tscn related ===')
for path, size in paths:
    if '1.tscn' in path or '1.' in path and 'char_select' in path:
        print(f'  {path} ({size})')

print()
print('=== .uid files ===')
for path, size in paths:
    if path.endswith('.uid'):
        print(f'  {path} ({size})')

print()
print('=== scenes/ full list ===')
for path, size in paths:
    if path.startswith('scenes/'):
        print(f'  {path} ({size})')
