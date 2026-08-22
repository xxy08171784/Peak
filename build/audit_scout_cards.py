#!/usr/bin/env python3
"""Fail-fast audit for Scout card text, hover tips, energy icons, and custom types."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CARDS_DIR = ROOT / "src" / "Core" / "Models" / "Cards"
POOL_PATH = ROOT / "src" / "Core" / "Models" / "CardPools" / "ScoutCardPool.cs"
LOCALIZATION_PATH = ROOT / "localization" / "zhs" / "cards.json"

ITEM_CARDS = {"Frisbee", "Conch", "Explosive", "Rope", "Cannon", "Coconut", "Snowball"}
POWER_TERMS = {
    "孢子": "SporePower",
    "炎热": "HeatPower",
    "寒冷": "ColdPower",
    "渐冻": "FrostbitePower",
    "易伤": "VulnerablePower",
    "虚弱": "WeakPower",
    "脆弱": "FrailPower",
    "敏捷": "DexterityPower",
    "力量": "StrengthPower",
    "覆甲": "PlatingPower",
    "无实体": "IntangiblePower",
    "再生": "RegenPower",
    "缓冲": "BufferPower",
    "灾厄": "DoomPower",
    "领队追杀": "LeadersPursuitPower",
    "疲劳": "TiredPower",
    "缠绕": "TwinePower",
}


def card_id(class_name: str) -> str:
    value = re.sub(r"(.)([A-Z][a-z]+)", r"\1_\2", class_name)
    value = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", value)
    return value.upper()


def uncomment(source: str) -> str:
    source = re.sub(r"/\*.*?\*/", "", source, flags=re.S)
    return re.sub(r"//.*", "", source)


def main() -> int:
    errors: list[str] = []
    localization = json.loads(LOCALIZATION_PATH.read_text(encoding="utf-8"))
    pool_source = POOL_PATH.read_text(encoding="utf-8")
    cards = sorted(set(re.findall(r"ModelDb\.Card<([A-Za-z0-9_]+)>", pool_source)) - {"T"})

    if not cards:
        errors.append("ScoutCardPool does not contain any cards")

    for class_name in cards:
        source_path = CARDS_DIR / f"{class_name}.cs"
        if not source_path.exists():
            errors.append(f"{class_name}: source file is missing")
            continue

        source = uncomment(source_path.read_text(encoding="utf-8"))
        localization_id = card_id(class_name)
        title_key = f"{localization_id}.title"
        description_key = f"{localization_id}.description"
        if title_key not in localization:
            errors.append(f"{class_name}: missing {title_key}")
        if description_key not in localization:
            errors.append(f"{class_name}: missing {description_key}")
            continue

        description = localization[description_key]
        if re.search(r"\[(?:red|green|purple|blue)\]", description):
            errors.append(f"{class_name}: mechanic highlights must use [gold]")

        used_powers = set(
            re.findall(r"(?:PowerCmd\.Apply|GetPower|PowerVar)<([A-Za-z0-9_]+)>", source)
        ) - {"T"}
        hover_powers = set(re.findall(r"HoverTipFactory\.FromPower<([A-Za-z0-9_]+)>", source))

        for power in sorted(used_powers - hover_powers):
            errors.append(f"{class_name}: {power} is used but has no hover tip")

        for term, power in POWER_TERMS.items():
            if term in description and power not in hover_powers:
                errors.append(f"{class_name}: highlighted term {term} has no {power} hover tip")

        if "中毒" in description:
            poison_power = "PoisonPower" if "PoisonPower" in source else "ZhongduPower"
            if poison_power not in hover_powers:
                errors.append(f"{class_name}: highlighted term 中毒 has no {poison_power} hover tip")

        gains_energy = "PlayerCmd.GainEnergy" in source
        has_energy_var = "new EnergyVar(" in source
        has_energy_hover = "EnergyHoverTip" in source or "HoverTipFactory.ForEnergy(" in source
        if gains_energy and not has_energy_hover:
            errors.append(f"{class_name}: gains energy but has no energy hover tip")
        if has_energy_var and "{Energy:energyIcons()}" not in description:
            errors.append(f"{class_name}: EnergyVar must render with energyIcons()")
        if re.search(r"PlayerCmd\.GainEnergy\(\s*\d+(?:\.\d+)?m", source):
            errors.append(f"{class_name}: fixed energy gain must use EnergyVar")

        class_decl = re.search(rf"class\s+{re.escape(class_name)}\s*:\s*([^\r\n{{]+)", source)
        bases = class_decl.group(1) if class_decl else ""
        if class_name in ITEM_CARDS and "IItemCard" not in bases:
            errors.append(f"{class_name}: required item card is missing IItemCard")
        if class_name == "ElasticMushroom":
            if "IItemCard" in bases:
                errors.append("ElasticMushroom: must remain a Power card, not an item card")
            if "CardType.Power" not in source:
                errors.append("ElasticMushroom: constructor must use CardType.Power")

    legacy_tag_patch = ROOT / "Patches" / "FoodCardTagPatch.cs"
    if legacy_tag_patch.exists():
        errors.append("legacy FoodCardTagPatch.cs must stay removed")

    if errors:
        print("Scout card audit FAILED:")
        for error in errors:
            print(f"- {error}")
        return 1

    print(
        f"Scout card audit passed: cards={len(cards)}, "
        f"required_items={len(ITEM_CARDS)}, legacy_food_tag=absent"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
