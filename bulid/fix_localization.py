# -*- coding: utf-8 -*-
"""一次性脚本：更新 scout 卡牌/能力 localization 文案（保持 UTF-8 无 BOM、保留原换行符）。"""

def load(path):
    with open(path, 'r', encoding='utf-8', newline='') as f:
        return f.read()

def save(path, s):
    with open(path, 'w', encoding='utf-8', newline='') as f:
        f.write(s)

# ---------- cards.json ----------
cards_path = r'C:\Users\shiqian\Peak\localization\zhs\cards.json'
c = load(cards_path)

# 梦开始的地方：升级后去掉文案里的手工"保留"字样（保留词条由 AddKeyword(Retain) 自动显示）
old_wdb = '"WHERE_DREAMS_BEGIN.description": "{IfUpgraded:show:[gold]保留[/gold]。\\n}切换到0[gold]海岛[/gold]。",'
new_wdb = '"WHERE_DREAMS_BEGIN.description": "切换到0[gold]海岛[/gold]。",'
assert old_wdb in c, "WDB description not found!"
c = c.replace(old_wdb, new_wdb)
save(cards_path, c)
print("cards.json OK")

# ---------- powers.json ----------
powers_path = r'C:\Users\shiqian\Peak\localization\zhs\powers.json'
p = load(powers_path)

# 孢子：修改文案描述
old = '"SPORE_POWER.description": "孢子层数。",'
new = '"SPORE_POWER.description": "一种参与debuff计数的无作用debuff（暂时）。",'
assert old in p, "SPORE_POWER.description not found!"
p = p.replace(old, new)

old = '"SPORE_POWER.smartDescription": "[green]孢子[/green][blue]{Amount}[/blue]层。",'
new = '"SPORE_POWER.smartDescription": "一种参与debuff计数的无作用debuff（暂时）。当前[blue]{Amount}[/blue]层。",'
assert old in p, "SPORE_POWER.smartDescription not found!"
p = p.replace(old, new)

# 疲劳：增加文案描述（回合开始时失去1点费用）
old = '"TIRED_POWER.description": "在你的回合开始时，失去[blue]1[/blue]点[gold]能量[/gold]。",'
new = '"TIRED_POWER.description": "在回合开始时失去1点费用。",'
assert old in p, "TIRED_POWER.description not found!"
p = p.replace(old, new)

old = '"TIRED_POWER.smartDescription": "在你的回合开始时，失去[blue]{Amount}[/blue]点[gold]能量[/gold]。",'
new = '"TIRED_POWER.smartDescription": "在回合开始时失去[blue]{Amount}[/blue]点费用。",'
assert old in p, "TIRED_POWER.smartDescription not found!"
p = p.replace(old, new)

save(powers_path, p)
print("powers.json OK")
