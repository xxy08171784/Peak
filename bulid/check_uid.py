# -*- coding: utf-8 -*-
import re

imp = open(r'C:\Users\shiqian\Peak\animations\character_select\scout\characterselect_scout.png.import','r',encoding='utf-8').read()
m = re.search(r'uid="([^"]+)"', imp)
print('import uid:', m.group(1) if m else 'NONE')

tscn = open(r'C:\Users\shiqian\Peak\scenes\screens\char_select\char_select_bg_scout.tscn','r',encoding='utf-8').read()
m2 = re.search(r'uid="([^"]+)"', tscn)
print('tscn uid:', m2.group(1) if m2 else 'NONE')
