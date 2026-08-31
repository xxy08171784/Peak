# -*- coding: utf-8 -*-
import json, io

# 游戏内置 zhs ancients.json
game = json.loads(io.open(r'C:\Users\shiqian\Peak\bulid\game_zhs_ancients.json', encoding='utf-8').read())
# mod 当前 ancients.json
mod = json.loads(io.open(r'C:\Users\shiqian\Peak\localization\zhs\ancients.json', encoding='utf-8').read())

print('game keys:', len(game))
print('mod keys:', len(mod))

# 冲突 key（两边都有）
conflict = set(game) & set(mod)
print('conflict keys:', len(conflict))
for k in sorted(conflict):
    print('  CONFLICT', k)
    print('    game:', repr(game[k])[:80])
    print('    mod :', repr(mod[k])[:80])

# 合并：mod 优先
merged = {}
merged.update(game)
merged.update(mod)
print('\nmerged keys:', len(merged))

# 检查最终文件中 SCOUT 相关 key
scout_keys = [k for k in merged if 'SCOUT' in k]
print('SCOUT keys in merged:', len(scout_keys))

# 写回
output = '{\n'
items = sorted(merged.items())
for i, (k, v) in enumerate(items):
    comma = ',' if i < len(items) - 1 else ''
    output += f'  "{k}": {json.dumps(v, ensure_ascii=False)}{comma}\n'
output += '}'
io.open(r'C:\Users\shiqian\Peak\localization\zhs\ancients.json', 'w', encoding='utf-8', newline='').write(output)
print('written to localization/zhs/ancients.json')

# 验证
check = json.loads(output)
print('verify parse OK, keys:', len(check))
