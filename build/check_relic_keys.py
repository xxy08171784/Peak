# -*- coding: utf-8 -*-
import io, json, re

# 提取 ScoutRelicPool.cs 里的遗物类名
pool_file = r'C:\Users\shiqian\Peak\src\Core\Models\RelicsPools\ScoutRelicPool.cs'
text = io.open(pool_file, encoding='utf-8').read()
relic_classes = re.findall(r'ModelDb\.Relic<(\w+)>\(\)', text)
print('relics in pool:', relic_classes)

# 加载 zhs/relics.json
relics_json = json.loads(io.open(r'C:\Users\shiqian\Peak\localization\zhs\relics.json', encoding='utf-8').read())

def camel_to_snake_upper(name):
    s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)
    return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).upper()

missing = []
for cls in relic_classes:
    rid = camel_to_snake_upper(cls)
    key_t = f'{rid}.title'
    key_d = f'{rid}.description'
    has_t = key_t in relics_json
    has_d = key_d in relics_json
    print(f'  {cls} -> {rid}: title={has_t} desc={has_d}')
    if not (has_t and has_d):
        missing.append((cls, rid, has_t, has_d))

print('\nMISSING relics:')
for cls, rid, ht, hd in missing:
    print(f'  {cls} -> {rid}: title={ht} desc={hd}')
