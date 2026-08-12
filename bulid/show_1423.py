# -*- coding: utf-8 -*-
import io

path = r'C:\Users\shiqian\Peak\localization\zhs\cards.json'
data = io.open(path, encoding='utf-8').read()
lines = data.split('\n')

# 显示 1418-1428 行（1-based）
for i in range(1418, 1429):
    print(i + 1, repr(lines[i]))
