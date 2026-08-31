# -*- coding: utf-8 -*-
import re

tscn = open(r'C:\Users\shiqian\Peak\scenes\screens\char_select\char_select_bg_scout.tscn','r',encoding='utf-8').read()
m = re.search(r'\[ext_resource[^\]]*uid="([^"]+)"[^\]]*\]', tscn)
print('tscn ext_resource uid:', m.group(1) if m else 'NONE')
print('tscn ext_resource line:', [l for l in tscn.split('\n') if 'ext_resource' in l][0])
