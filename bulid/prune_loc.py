# -*- coding: utf-8 -*-
# 方案B：精简 mod 本地化 —— 删除所有与游戏新版重合的原版 key（交给游戏显示）
# 保留：mod 自建 key（游戏表没有的）+ 特例（涅奥开场白）
import json, io, os

MOD_DIR = r'C:\Users\shiqian\Peak\localization\zhs'
GAME_DIR = r'C:\Users\shiqian\Peak\bulid\game_loc_new'

# 特例：mod 特意定制的覆盖 key（保留）
KEEP_SPECIAL = {
    'ancients.json': {
        'NEOW.talk.firstVisitEver.0-0.ancient',  # 涅奥开场白（mod 定制）
    },
}

FILES = ['cards.json', 'relics.json', 'powers.json', 'ancients.json',
         'potions.json', 'characters.json', 'epochs.json', 'card_library.json']

total_removed = 0
for f in FILES:
    mod_path = os.path.join(MOD_DIR, f)
    game_path = os.path.join(GAME_DIR, f)
    if not os.path.exists(game_path):
        print(f'{f}: 游戏无此表，跳过')
        continue
    mod = json.loads(io.open(mod_path, encoding='utf-8').read())
    game = json.loads(io.open(game_path, encoding='utf-8').read())
    keep_special = KEEP_SPECIAL.get(f, set())

    before = len(mod)
    removed = []
    for k in list(mod.keys()):
        if k in game and k not in keep_special:
            del mod[k]
            removed.append(k)
    total_removed += len(removed)

    # 写回（UTF-8 无 BOM，保持中文不转义）
    with io.open(mod_path, 'w', encoding='utf-8', newline='') as fh:
        fh.write(json.dumps(mod, ensure_ascii=False, indent=2))

    # 验证（特例保留 key 不计入重合）
    overlap = [k for k in mod if k in game and k not in keep_special]
    assert not overlap, f'{f}: 仍有 {len(overlap)} 个 key 与游戏重合! {overlap[:5]}'
    print(f'{f}: {before} -> {len(mod)} (删除 {len(removed)})  剩余与游戏重合=0 [OK]')

print(f'\n总计删除 {total_removed} 个原版 key')
print('完成')
