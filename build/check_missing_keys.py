# -*- coding: utf-8 -*-
import io, json, re, os, glob

# 提取 ScoutCardPool.cs 里的卡类名
pool_file = r'C:\Users\shiqian\Peak\src\Core\Models\CardPools\ScoutCardPool.cs'
text = io.open(pool_file, encoding='utf-8').read()
card_classes = re.findall(r'ModelDb\.Card<(\w+)>\(\)', text)
print('total cards in pool:', len(card_classes))

# 加载 zhs/cards.json
cards_json = json.loads(io.open(r'C:\Users\shiqian\Peak\localization\zhs\cards.json', encoding='utf-8').read())

# 每个卡的 Id 怎么得到？卡类名转成 ID 规则：类名 → 大写蛇形（如 StrikeScout → STRIKE_SCOUT）
def camel_to_snake_upper(name):
    s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)
    return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).upper()

missing = []
for cls in card_classes:
    card_id = camel_to_snake_upper(cls)
    # 特殊 ID 覆盖（如 TheWrathOfTheFoodGod → THE_WRATH_OF_THE_FOOD_GOD 需要确认）
    key_t = f'{card_id}.title'
    key_d = f'{card_id}.description'
    has_t = key_t in cards_json
    has_d = key_d in cards_json
    if not (has_t and has_d):
        missing.append((cls, card_id, has_t, has_d))

print('\nMISSING cards (class, id, hasTitle, hasDesc):')
for cls, cid, ht, hd in missing:
    print(f'  {cls} -> {cid}: title={ht} desc={hd}')

# 检查特殊 ID 转换
print('\nSpecial check:')
for cls in ['TheWrathOfTheFoodGod', 'AllIntoBackpack', 'InitialSupplies', 'BreakDown', 'ReachThePeak', 'TheMountainOfFlames', 'ShareMisfortune', 'CheckpointFlag', 'ExperienceAndToughening']:
    cid = camel_to_snake_upper(cls)
    print(f'  {cls} -> {cid} : title={"OK" if cid+".title" in cards_json else "MISSING"}')
