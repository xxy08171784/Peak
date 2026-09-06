# -*- coding: utf-8 -*-
# 修复：mod 本地化 = 游戏新版完整副本 + mod 自建 key + 特例覆盖
import json, io, os

MOD_DIR = r'C:\Users\shiqian\Peak\localization\zhs'
GAME_DIR = r'C:\Users\shiqian\Peak\bulid\game_loc_new'

# 特例：mod 特意定制的覆盖 key（强制用 mod 版）
KEEP_SPECIAL = {
    'ancients.json': {
        'NEOW.talk.firstVisitEver.0-0.ancient',  # 涅奥开场白（mod 定制）
    },
}

FILES = ['cards.json', 'relics.json', 'powers.json', 'ancients.json',
         'potions.json', 'characters.json', 'epochs.json', 'card_library.json']

for f in FILES:
    mod_path = os.path.join(MOD_DIR, f)
    game_path = os.path.join(GAME_DIR, f)
    if not os.path.exists(game_path):
        print(f'{f}: 游戏无此表，跳过')
        continue
    mod = json.loads(io.open(mod_path, encoding='utf-8').read())
    game = json.loads(io.open(game_path, encoding='utf-8').read())
    keep_special = KEEP_SPECIAL.get(f, set())

    # base = 游戏新版完整副本
    merged = dict(game)
    added = 0
    overridden = 0
    for k, v in mod.items():
        if k in merged:
            if k in keep_special:
                merged[k] = v  # 特例强制覆盖
                overridden += 1
            # 其他重合 key：以游戏新版为准（不覆盖）
        else:
            merged[k] = v  # mod 自建 key
            added += 1

    with io.open(mod_path, 'w', encoding='utf-8', newline='') as fh:
        fh.write(json.dumps(merged, ensure_ascii=False, indent=2))

    print(f'{f}: 游戏{len(game)} + mod新增{added} + 特例覆盖{overridden} = {len(merged)}')

print('完成')
