# Peak 1.0.5 custom card types

## Single source of truth

- Every `IFoodCard` model is exposed as the custom `Food` card type.
- Every `IItemCard` model that is not also food is exposed as the custom `Item` card type.
- `Panacea` implements both marker interfaces. Its primary visible type is `Food`, while its item marker remains available to Peak mechanics.
- New cards gain the matching type by implementing the marker interface; constructors keep their original vanilla behavior category.
- Food and item marker interfaces are internal gameplay contracts only. They must not append legacy `食物。` / `物品。` lines, create hover-tip definitions, or register parallel search tags; the custom card type plaque is the only visible classification.
- The current explicit item set also includes Frisbee, Conch, Explosive, Rope, Cannon, Coconut, and Snowball. Elastic Mushroom deliberately remains a vanilla Power card.

## Compatibility rules

- Cards whose constructor declared `CardType.Power` keep the vanilla one-play Power lifecycle after their visible type becomes Food or Item.
- Custom type localization lives under `peak/localization/<locale>/gameplay_ui.json`. A partial table at `localization/<locale>/gameplay_ui.json` would replace the game's complete table and expose raw localization keys throughout the compendium.
- Custom portrait-border PNGs use the vanilla blue rarity baseline. The game's uncommon material is identity, rare rotates that blue baseline to orange, and common desaturates it to gray. A red source border produces the incorrect red-uncommon/blue-rare sequence.
- Missing custom enums, assets, or an empty marker audit are hard failures. They must not silently fall back to vanilla visuals.

## Runtime audit

After `ModelDb.Init`, Peak logs the assigned enum values and counts every marked card, including food, item-only, dual-marker, and declared-Power totals. A mismatch throws immediately so a partial type migration cannot appear successful.

## Card text and hover audit

- Named mechanics use `[gold]` consistently, including debuffs such as Weak, Frail, and Vulnerable.
- Every power read, applied, or exposed through a `PowerVar` must have a matching `HoverTipFactory.FromPower<T>()` entry.
- Fixed energy gains use `EnergyVar`, render through `{Energy:energyIcons()}`, and include the standard energy hover tip.
- Run `py -3 build/audit_scout_cards.py` before packaging. It fails on missing Scout localization, non-gold mechanic highlights, missing power/energy hover tips, fixed literal energy gains, the explicit item-type contract, or restoration of the legacy food-tag patch.

## Visual acceptance

Open the Scout card compendium and confirm:

1. Vanilla type and sort labels are translated rather than shown as `gameplay_ui.*` keys.
2. All food and item cards use their matching type label and frame, not only Roast Chicken.
3. Normal attack, skill, and power cards retain their vanilla labels and frames.
4. Food/item cards that originally behaved as powers still leave the hand after use and apply their effect once.
5. Food/item cards do not repeat their type as a colored line in the rules text and do not produce a second type hover tip.
