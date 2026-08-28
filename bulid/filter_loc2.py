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
        mod_text = fh.read()
    mod_data = json.loads(mod_text)

    mod_only = {}
    if os.path.exists(gp):
        with open(gp, 'r', encoding='utf-8') as fh:
            game_text = fh.read()
        game_data = json.loads(game_text)
        for k, v in mod_data.items():
            if k not in game_data:
                mod_only[k] = v
    else:
        mod_only = mod_data

    # Merge with existing peak file if any
    if os.path.exists(op):
        with open(op, 'r', encoding='utf-8') as fh:
            existing_text = fh.read()
        if existing_text.strip() and existing_text.strip() != '{}':
            existing_data = json.loads(existing_text)
            for k, v in mod_only.items():
                if k not in existing_data:
                    existing_data[k] = v
            mod_only = existing_data

    os.makedirs(out_dir, exist_ok=True)
    with open(op, 'w', encoding='utf-8') as fh:
        json.dump(mod_only, fh, indent=2, ensure_ascii=False)
        fh.write('\n')
    print(f'{f}: total={len(mod_data)} game_only={len(mod_data)-len(mod_only)} mod_only={len(mod_only)}')