# -*- coding: utf-8 -*-
import struct, json, io

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# 找到 zhs/cards.json 和 eng/cards.json 的文件内容
# PCK 目录在文件开头，文件数据在末尾。文件名后面紧跟 offset, size, md5, flags
# 解析目录
pos = 4
fmt_ver = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_major = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_minor = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
ver_patch = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
pack_flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
file_count = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
print(f'fmt={fmt_ver} godot={ver_major}.{ver_minor}.{ver_patch} flags={pack_flags} files={file_count}')

if pack_flags & 1:
    key = data[pos:pos+32]; pos += 32
    print('encrypted, key len 32')

targets = {}
for i in range(file_count):
    plen = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    if plen > 5000 or plen <= 0:
        print(f'BAD path_len {plen} at file {i}')
        break
    path = data[pos:pos+plen].decode('utf-8', errors='replace'); pos += plen
    offset = struct.unpack('<q', data[pos:pos+8])[0]; pos += 8
    size = struct.unpack('<q', data[pos:pos+8])[0]; pos += 8
    md5 = data[pos:pos+16].hex(); pos += 16
    flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    if 'localization' in path:
        targets[path] = (offset, size)

print('localization files:')
for p, (off, size) in targets.items():
    print(f'  {p}  offset={off} size={size}')
