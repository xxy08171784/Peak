# -*- coding: utf-8 -*-
# 从游戏 pck 中提取 card_library 相关资源文件
import struct, os, sys

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()
print('total size', len(data))

pos = 4
format_ver = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_major = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_minor = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_patch = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
pack_flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
file_count = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
print('Format:', format_ver, 'Godot', ver_major, ver_minor, ver_patch, 'flags', pack_flags, 'files', file_count)

if pack_flags & 1:
    key = data[pos:pos+32]; pos += 32
    print('Encrypted pack')

outdir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'extracted')
os.makedirs(outdir, exist_ok=True)

count = 0
for i in range(file_count):
    path_len = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    path = data[pos:pos+path_len].decode('utf-8', errors='replace'); pos += path_len
    offset = struct.unpack('<q', data[pos:pos+8])[0]; pos += 8
    size = struct.unpack('<q', data[pos:pos+8])[0]; pos += 8
    md5 = data[pos:pos+16].hex(); pos += 16
    flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    low = path.lower()
    if 'card_library' in low or 'cardlibrary' in low:
        print('FOUND:', path, 'size', size)
        count += 1
        # 保存文件
        rel = path.lstrip('res://').replace('/', os.sep)
        dst = os.path.join(outdir, rel)
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        with open(dst, 'wb') as f:
            f.write(data[offset:offset+size])
        print('  saved ->', dst)

print('done, found', count)
