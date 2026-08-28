# -*- coding: utf-8 -*-
# 研究 PCK v3 文件表结构
pck_path = r'D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck'
data = open(pck_path, 'rb').read()

# header: GDPC(4) ver(4)=3 major(4) minor(4) patch(4) flags(4) count(4)
# flags=2 -> PACK_REL_FILEBASE? 实际看 v3 是否有额外字段
pos = 28
print('header bytes:', data[pos:pos+80].hex())

# 尝试解析第一个文件条目
# v2: path_len(4) path offset(8) size(8) md5(16) flags(4)
# v3: 可能不同
import struct
p = pos
for i in range(5):
    path_len = struct.unpack('<I', data[p:p+4])[0]
    print(f'file[{i}] path_len={path_len} @{p}')
    path = data[p+4:p+4+path_len].decode('utf-8', errors='replace')
    print('   path:', path)
    p += 4 + path_len
    # 尝试多种格式
    print('   next 48 bytes:', data[p:p+48].hex())
    off, size = struct.unpack('<qq', data[p:p+16])
    print('   off,size =', off, size)
    break
