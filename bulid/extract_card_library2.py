# -*- coding: utf-8 -*-
# PCK v3/v4 格式解析：header 含 file_base(8) + dir_offset(8)
# 从游戏 pck 提取 card_library 相关资源
import struct, os

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
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

outdir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'extracted')
os.makedirs(outdir, exist_ok=True)

want = ['card_library', 'pool_toggle', 'char_select_scout', 'character_select']
count = 0
for i in range(file_count):
    path_len = struct.unpack('<I', data[p:p+4])[0]; p += 4
    path = data[p:p+path_len].decode('utf-8', errors='replace').split('\x00')[0]; p += path_len
    ofs = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    size = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    md5 = data[p:p+16].hex(); p += 16
    flags = struct.unpack('<I', data[p:p+4])[0]; p += 4
    low = path.lower()
    if any(w in low for w in want):
        print(f'FOUND: {path} (ofs={ofs} size={size} flags={flags})')
        count += 1
        rel = path.lstrip('res://').replace('/', os.sep)
        dst = os.path.join(outdir, rel)
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        with open(dst, 'wb') as f:
            f.write(data[file_base + ofs: file_base + ofs + size])
        print(f'  saved -> {dst}')

print('done, found', count)
