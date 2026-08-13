# -*- coding: utf-8 -*-
import hashlib, re, os

src = open(r'C:\Users\shiqian\Peak\animations\character_select\scout\characterselect_scout.png','rb').read()
print('source md5:', hashlib.md5(src).hexdigest())

imp = open(r'C:\Users\shiqian\Peak\animations\character_select\scout\characterselect_scout.png.import','r',encoding='utf-8').read()
m = re.search(r'path="res://\.godot/imported/characterselect_scout\.png-([0-9a-f]+)\.ctex"', imp)
print('import hash:', m.group(1) if m else 'not found')

for f in os.listdir(r'C:\Users\shiqian\Peak\.godot\imported'):
    if 'characterselect_scout' in f:
        full = os.path.join(r'C:\Users\shiqian\Peak\.godot\imported', f)
        print('cache file:', f, os.path.getmtime(full))
