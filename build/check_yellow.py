import struct

with open('peak.pck', 'rb') as f:
    data = f.read()
magic = data[0:4]
print('Magic:', magic)
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
    print('Encrypted')
found = False
for i in range(file_count):
    path_len = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    path = data[pos:pos+path_len].decode('utf-8', errors='replace'); pos += path_len
    offset = struct.unpack('<q', data[pos:pos+8])[0]; pos += 8
    size = struct.unpack('<q', data[pos:pos+8])[0]; pos += 8
    md5 = data[pos:pos+16].hex(); pos += 16
    flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    if 'yellow' in path or 'materials/cards/frames' in path:
        print('  [%d] %s size=%d' % (i, path, size))
        found = True
if not found:
    print('  NO yellow/frames files found!')
