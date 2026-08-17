# -*- coding: utf-8 -*-
# 检查 mod pck 中带 peak/ 前缀的路径
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

peak_prefix = 0
non_peak = 0
for i in range(file_count):
    path_len = struct.unpack('<I', data[p:p+4])[0]; p += 4
    path = data[p:p+path_len].decode('utf-8', errors='replace').split('\x00')[0]; p += path_len
    ofs = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    size = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    p += 16
    flags = struct.unpack('<I', data[p:p+4])[0]; p += 4
    if path.startswith('peak/'):
        peak_prefix += 1
        if peak_prefix <= 15:
            print('  peak/:', path)
    elif path.startswith('res://'):
        print('  res://', path)
        non_peak += 1

print(f'peak/ prefix files: {peak_prefix}, res:// files: {non_peak}')
