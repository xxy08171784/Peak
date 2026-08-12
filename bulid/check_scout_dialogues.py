# -*- coding: utf-8 -*-
import json, io, re, os, glob

# 加载合并后的 ancients.json
ancients = json.loads(io.open(r'C:\Users\shiqian\Peak\localization\zhs\ancients.json', encoding='utf-8').read())

# 检查 SCOUT 对话 key 是否完整：每个 ancient 的 SCOUT 对话
# 找出所有 .talk.SCOUT 的 key
scout_talk = {}
for k in ancients:
    m = re.match(r'^(\w+)\.talk\.SCOUT\.(\d+)-(\d+)(r?)\.(ancient|char|next)$', k)
    if m:
        ancient, d_idx, l_idx, rep, kind = m.groups()
        key = f'{ancient}.talk.SCOUT.{d_idx}'
        scout_talk.setdefault(key, {})[f'{l_idx}{rep}.{kind}'] = True

print('SCOUT dialogue groups:')
for k in sorted(scout_talk):
    lines = scout_talk[k]
    # 计算行数（ancient + char 的行数）
    line_keys = [lk for lk in lines if lk.endswith('.ancient') or lk.endswith('.char')]
    print(f'  {k}: lines={len(line_keys)} keys={sorted(lines)}')
