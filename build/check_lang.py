# -*- coding: utf-8 -*-
import io, json, os, glob

base = r'C:\Users\shiqian\Peak\localization'

# 找 eng 和 zhs 目录
print('dirs:', os.listdir(base))

eng_dir = os.path.join(base, 'eng')
zhs_dir = os.path.join(base, 'zhs')
print('eng exists:', os.path.isdir(eng_dir))
print('zhs exists:', os.path.isdir(zhs_dir))

if os.path.isdir(eng_dir):
    for f in sorted(glob.glob(os.path.join(eng_dir, '*.json'))):
        print('  eng file:', os.path.basename(f))

if os.path.isdir(zhs_dir):
    for f in sorted(glob.glob(os.path.join(zhs_dir, '*.json'))):
        print('  zhs file:', os.path.basename(f))
