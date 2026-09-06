import struct
import sys

def parse_pck(path):
    with open(path, 'rb') as f:
        data = f.read()
    
    # Godot 4 PCK format
    magic = data[0:4]
    print(f"Magic: {magic}")
    
    # Try Godot 4 format: magic(4) + format_ver(4) + ver_major(4) + ver_minor(4) + ver_patch(4) + pack_flags(4) + file_count(4)
    pos = 4
    format_ver = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    ver_major = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    ver_minor = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    ver_patch = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    pack_flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    file_count = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
    print(f"Format: {format_ver}, Godot {ver_major}.{ver_minor}.{ver_patch}, flags={pack_flags}, files={file_count}")
    
    if pack_flags & 1:
        # encrypted pack - has key
        key = data[pos:pos+32]
        pos += 32
        print("Encrypted pack")
    
    files = []
    for i in range(file_count):
        path_len = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
        if path_len > 100000:
            print(f"  [{i}] BAD path_len {path_len} at pos {pos}")
            break
        path = data[pos:pos+path_len].decode('utf-8', errors='replace'); pos += path_len
        offset = struct.unpack('<q', data[pos:pos+8])[0]; pos += 8
        size = struct.unpack('<q', data[pos:pos+8])[0]; pos += 8
        md5 = data[pos:pos+16].hex(); pos += 16
        flags = struct.unpack('<I', data[pos:pos+4])[0]; pos += 4
        files.append((path, offset, size, flags))
    
    print(f"Parsed {len(files)} files")
    return files

if __name__ == '__main__':
    path = sys.argv[1]
    filter_str = sys.argv[2] if len(sys.argv) > 2 else None
    files = parse_pck(path)
    for (p, off, size, flags) in files:
        if filter_str is None or filter_str.lower() in p.lower():
            print(f"  {p}  (offset={off} size={size} flags={flags})")
