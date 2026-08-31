# -*- coding: utf-8 -*-
# 验证新 PCK：无 bulid/extracted、无原版资源名污染
import struct

pck_path = r'c:\Users\shiqian\Peak\bulid\peak_new.pck'
data = open(pck_path, 'rb').read()

pos = 4
version = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
for _ in range(3):
    pos += 4
pack_flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
file_base = struct.unpack('<Q', data[pos:pos+8])[0]; pos += 8
dir_offset = struct.unpack('<Q', data[pos:pos+8])[0]; pos += 8

p = dir_offset
file_count = struct.unpack('<I', data[p:p+4])[0]; p += 4

bad = []
godot = []
for i in range(file_count):
    path_len = struct.unpack('<I', data[p:p+4])[0]; p += 4
    path = data[p:p+path_len].decode('utf-8', errors='replace').split('\x00')[0]; p += path_len
    ofs = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    size = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    p += 16
    flags = struct.unpack('<I', data[p:p+4])[0]; p += 4
    if 'bulid' in path or 'extracted' in path.lower() or 'character_select_screen' in path or 'library_pool_toggle' in path or 'characterselect_' in path or 'card_library.tscn' in path:
        bad.append((path, size))
    if path.startswith('.godot/'):
        godot.append((path, size))

print('BAD (bulid/extracted/原版资源):', len(bad))
for b in bad:
    print('  !', b)

print()
print('.godot files in pck:', len(godot))
for g in godot[:25]:
    print('  ', g)
