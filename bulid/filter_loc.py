import json, os, re

game_dir = r'c:\Users\shiqian\Peak\bulid\game_loc_new'
mod_dir = r'c:\Users\shiqian\Peak\localization\zhs'
out_dir = r'c:\Users\shiqian\Peak\peak\localization\zhs'

files = ['cards', 'powers', 'relics', 'ancients', 'characters', 'epochs', 'potions', 'card_library', 'rest_site_ui']
for f in files:
    gp = os.path.join(game_dir, f + '.json')
    mp = os.path.join(mod_dir, f + '.json')
    op = os.path.join(out_dir, f + '.json')

    if not os.path.exists(mp):
        print(f'{f}: mod source missing')
        continue

    with open(mp, 'r', encoding='utf-8') as fh:
        mod_data = json.load(fh)

    mod_only = {}
    if os.path.exists(gp):
        with open(gp, 'r', encoding='utf-8') as fh:
            game_data = json.load(fh)
        for k, v in mod_data.items():
            if k not in game_data:
                mod_only[k] = v
    else:
        mod_only = mod_data

    print(f'{f}: total={len(mod_data)} mod_only={len(mod_only)}')

    # Merge with existing peak file if any
    if os.path.exists(op):
        with open(op, 'r', encoding='utf-8') as fh:
            existing = json.load(fh)
        for k, v in mod_only.items():
            if k not in existing:
                existing[k] = v
        mod_only = existing
        print(f'  peak/{f}.json merged={len(mod_only)}')

    os.makedirs(out_dir, exist_ok=True)
    with open(op, 'w', encoding='utf-8') as fh:
        json.dump(mod_only, fh, indent=2, ensure_ascii=False)
        fh.write('\n')
    print(f'  -> wrote {len(mod_only)} keys')