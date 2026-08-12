# -*- coding: utf-8 -*-
import io, re, glob, os

patches_dir = r'C:\Users\shiqian\Peak\Patches'
for f in sorted(glob.glob(os.path.join(patches_dir, '*ScoutDialoguePatch.cs'))):
    text = io.open(f, encoding='utf-8').read()
    name = os.path.basename(f)
    # 提取 class 名和 VisitIndex
    cls = re.search(r'class (\w+)', text)
    # 提取所有 new AncientDialogue 及其参数
    dialogues = re.findall(r'new AncientDialogue\(([^)]*)\)\s*\{[^}]*VisitIndex = (\d+)', text)
    print(f'== {name} ({cls.group(1) if cls else "?"}) ==')
    for params, visit in dialogues:
        nlines = len([p for p in params.split(',') if p.strip()]) if params.strip() else 0
        print(f'  VisitIndex={visit}: {nlines} lines')
