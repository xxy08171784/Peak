# -*- coding: utf-8 -*-
# 验证新 PCK 包含修复的图片资源
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

targets = ['red_hot_power', 'magic_bean_power', 'mountaineering_expert', 'sprite_potion']
found = {t: [] for t in targets}
bad = []
for i in range(file_count):
    path_len = struct.unpack('<I', data[p:p+4])[0]; p += 4
    path = data[p:p+path_len].decode('utf-8', errors='replace').split('\x00')[0]; p += path_len
    ofs = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    size = struct.unpack('<Q', data[p:p+8])[0]; p += 8
    p += 16
    flags = struct.unpack('<I', data[p:p+4])[0]; p += 4
    for t in targets:
        if t in path:
            found[t].append(path)
    if 'angry_power' in path or 'magic_bean_juice_power' in path or ('atlases/potion_atlas.sprites/sprite.tres' in path):
        bad.append(path)

for t in targets:
    print(f'=== {t}: {len(found[t])} files ===')
    for f in found[t][:8]:
        print(f'  {f}')

print()
print('BAD (旧名残留):', len(bad))
for b in bad:
    print('  !', b)
