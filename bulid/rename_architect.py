# -*- coding: utf-8 -*-
import json, io

path = r'C:\Users\shiqian\Peak\localization\zhs\ancients.json'
ancients = json.loads(io.open(path, encoding='utf-8').read())

# 重命名 ARCHITECT.talk.SCOUT.* -> THE_ARCHITECT.talk.SCOUT.*
renamed = {}
for k, v in ancients.items():
    if k.startswith('ARCHITECT.talk.SCOUT.'):
        new_k = 'THE_ARCHITECT.talk.SCOUT.' + k[len('ARCHITECT.talk.SCOUT.'):]
        renamed[new_k] = v
        print(f'RENAMED {k} -> {new_k}')

# 删除旧的 ARCHITECT.talk.SCOUT keys
for k in list(ancients):
    if k.startswith('ARCHITECT.talk.SCOUT.'):
        del ancients[k]

# 加入重命名的
for k, v in renamed.items():
    ancients[k] = v

print('\nafter rename, total keys:', len(ancients))
print('THE_ARCHITECT.talk.SCOUT keys:', len([k for k in ancients if k.startswith('THE_ARCHITECT.talk.SCOUT.')]))
print('ARCHITECT.talk.SCOUT keys:', len([k for k in ancients if k.startswith('ARCHITECT.talk.SCOUT.')]))

# 写回（保持排序）
output = '{\n'
items = sorted(ancients.items())
for i, (k, v) in enumerate(items):
    comma = ',' if i < len(items) - 1 else ''
    output += f'  "{k}": {json.dumps(v, ensure_ascii=False)}{comma}\n'
output += '}'
io.open(path, 'w', encoding='utf-8', newline='').write(output)
print('written')

# 验证
check = json.loads(output)
print('verify OK, keys:', len(check))
