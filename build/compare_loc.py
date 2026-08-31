# -*- coding: utf-8 -*-
# 对比 mod localization/zhs 与新版游戏提取物 game_loc_new，找出文案差异
import json, io, os

MOD_DIR = r'C:\Users\shiqian\Peak\localization\zhs'
GAME_DIR = r'C:\Users\shiqian\Peak\bulid\game_loc_new'

# mod 8 个文件对应的游戏文件
files = ['cards.json', 'relics.json', 'powers.json', 'ancients.json',
         'potions.json', 'characters.json', 'epochs.json', 'card_library.json']

def load(d, f):
    p = os.path.join(d, f)
    if not os.path.exists(p):
        return None
    return json.loads(io.open(p, encoding='utf-8').read())

total_diff = 0
for f in files:
    mod = load(MOD_DIR, f)
    game = load(GAME_DIR, f)
    if mod is None:
        print(f'== {f}: mod 无此文件（游戏有 {len(game)} keys）')
        continue
    if game is None:
        print(f'== {f}: 游戏无此文件（mod 有 {len(mod)} keys）')
        continue
    diffs = []
    for k, v in mod.items():
        if k in game and game[k] != v:
            diffs.append((k, v, game[k]))
    only_mod = sorted(set(mod) - set(game))
    only_game = sorted(set(game) - set(mod))
    print(f'== {f}: mod={len(mod)} game={len(game)} 值不同={len(diffs)} mod独有={len(only_mod)} game独有={len(only_game)}')
    for k, mv, gv in sorted(diffs):
        print(f'  DIFF {k}')
        print(f'    mod : {mv!r}')
        print(f'    game: {gv!r}')
    if only_mod:
        print(f'  [mod 独有] {", ".join(only_mod[:20])}{" ..." if len(only_mod)>20 else ""}')
    if only_game:
        print(f'  [game 独有] {", ".join(only_game[:20])}{" ..." if len(only_game)>20 else ""}')
    total_diff += len(diffs)
print(f'\n总计值不同: {total_diff}')
