# -*- coding: utf-8 -*-
import json, io, re, glob, os

ancients = json.loads(io.open(r'C:\Users\shiqian\Peak\localization\zhs\ancients.json', encoding='utf-8').read())

# ancient -> 对话列表索引 -> 行数
# 从 patch 文件提取
patches_dir = r'C:\Users\shiqian\Peak\Patches'
results = []
for f in sorted(glob.glob(os.path.join(patches_dir, '*ScoutDialoguePatch.cs'))):
    text = io.open(f, encoding='utf-8').read()
    name = os.path.basename(f)
    ancient = name.replace('ScoutDialoguePatch.cs', '').upper()
    if ancient == 'ARCHITECT':
        ancient = 'THE_ARCHITECT'
    # 提取 dialogues
    dialogues = re.findall(r'new AncientDialogue\(([^)]*)\)\s*\{[^}]*VisitIndex = (\d+)', text)
    for idx, (params, visit) in enumerate(dialogues):
        nlines = len([p for p in params.split(',') if p.strip()]) if params.strip() else 0
        # 检查该 dialogue 每行的 key 是否存在
        missing = []
        for l in range(nlines):
            k_ancient = f'{ancient}.talk.SCOUT.{idx}-{l}.ancient'
            k_char = f'{ancient}.talk.SCOUT.{idx}-{l}.char'
            if k_ancient not in ancients and k_char not in ancients:
                missing.append(f'{idx}-{l}')
        status = 'OK' if not missing else f'MISSING lines {missing}'
        results.append((ancient, idx, visit, nlines, status))

print(f'{"Ancient":<15} {"Idx":<4} {"Visit":<6} {"Lines":<6} Status')
for ancient, idx, visit, nlines, status in results:
    print(f'{ancient:<15} {idx:<4} {visit:<6} {nlines:<6} {status}')
