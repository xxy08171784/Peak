# -*- coding: utf-8 -*-
import io

path = r'C:\Users\shiqian\Peak\localization\zhs\cards.json'
data = io.open(path, encoding='utf-8').read()
lines = data.split('\n')

# 删除 1-based 1371-1375 行 => index 1370-1374
del lines[1370:1375]

result = '\n'.join(lines)
io.open(path, 'w', encoding='utf-8', newline='').write(result)
print('done, new total lines:', len(lines))

# 验证删除区域
for i in range(1366, 1374):
    print(i + 1, repr(lines[i]))
