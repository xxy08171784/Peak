import struct

f = open(r'c:\Users\shiqian\Peak\bulid\peak_new.pck', 'rb')
magic = f.read(4)
fmt = struct.unpack('<I', f.read(4))[0]
maj, min_, pat, flags, count = struct.unpack('<IIIII', f.read(20))
if flags & 1:
    f.read(32)
found = []
for i in range(count):
    plen = struct.unpack('<I', f.read(4))[0]
    p = f.read(plen).decode('utf-8', errors='replace')
    f.read(8 + 8 + 16 + 4)
    if 'merchant' in p or 'store' in p or 'ScoutMerchant' in p:
        found.append(p)
print('TOTAL:', count)
for x in found:
    print(x)
