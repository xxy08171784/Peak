# -*- coding: utf-8 -*-
import json, io

backup = json.loads(io.open(r'C:\Users\shiqian\Peak\bulid\cards_backup.json', encoding='utf-8').read())
current = json.loads(io.open(r'C:\Users\shiqian\Peak\localization\zhs\cards.json', encoding='utf-8').read())

bk = set(backup.keys())
cur = set(current.keys())

lost = bk - cur
added = cur - bk

print('backup keys:', len(bk))
print('current keys:', len(cur))
print('LOST keys (in backup but not current):', len(lost))
for k in sorted(lost):
    print('  LOST', k, '=', repr(backup[k])[:100])
print('\nADDED keys (in current but not backup):', len(added))
