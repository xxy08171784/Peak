# -*- coding: utf-8 -*-
"""Extract original energy counter scene from the game's main PCK for reference."""
import struct, sys

pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
out_path = r'C:\Users\shiqian\Peak\bulid\decomp\original_energy_counter.tscn'
filter_str = 'energy_counter'

data = open(pck_path, 'rb').read()
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

matches = []
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
    if filter_str in path.lower():
        matches.append((path, offset, size))

print(f'Found {len(matches)} matches:')
for p, off, size in matches:
    print(f'  {p}  offset={off} size={size}')
    try:
        content = data[off:off+size]
        with open(out_path, 'wb') as f:
            f.write(content)
        print(f'    -> wrote {out_path}')
    except Exception as e:
        print(f'    ERROR: {e}')
