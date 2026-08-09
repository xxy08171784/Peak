"""生成 scout_frames.tres：包含 default(待机34帧) 和 attack(攻击27帧) 两个动画。
待机帧来自 images/animation/scout_idle/，攻击帧来自 images/animation/scout_attack/。
速度已按 2 倍速设置：default=30, attack=40。
"""
import os
import re

ROOT = r"C:\Users\shiqian\Peak"

def get_uid(png_rel):
    """从 .import 文件读取 UID"""
    imp = os.path.join(ROOT, png_rel + ".import")
    with open(imp, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()
    m = re.search(r'uid="([^"]+)"', content)
    if not m:
        raise ValueError(f"no uid in {imp}")
    return m.group(1)

# 待机帧（animation/scout_idle）
idle_frames = []
for i in range(34):
    png = f"images/animation/scout_idle/skeleton_scout_{i:02d}.png"
    idle_frames.append((png, get_uid(png)))

# 攻击帧（scout_attack）
attack_frames = []
for i in range(27):
    png = f"images/animation/scout_attack/skeleton-attack_{i:02d}.png"
    attack_frames.append((png, get_uid(png)))

def ext_resource_block(frames, start_id=0):
    """生成 [ext_resource] 块和引用 ID（从 start_id 开始编号，避免重复）"""
    lines = []
    ids = []
    for idx, (png, uid) in enumerate(frames):
        rid = f"id_{start_id + idx}"
        ids.append(rid)
        lines.append(f'[ext_resource type="Texture2D" uid="{uid}" path="res://{png}" id="{rid}"]')
    return lines, ids

lines = []
total = len(idle_frames) + len(attack_frames)
lines.append(f'[gd_resource type="SpriteFrames" load_steps={total+1} format=3 uid="uid://c6d0hqdyr227s"]')
lines.append("")

# 待机帧 ext_resource（id_0 起）
idle_lines, idle_ids = ext_resource_block(idle_frames, 0)
lines.extend(idle_lines)
lines.append("")

# 攻击帧 ext_resource（id_34 起，避开 idle 的 id）
attack_lines, attack_ids = ext_resource_block(attack_frames, len(idle_frames))
lines.extend(attack_lines)
lines.append("")

lines.append("[resource]")
lines.append("animations = [")

# default 动画（速度 2 倍：30）
lines.append('{')
lines.append('"frames": [{')
frame_strs = []
for rid in idle_ids:
    frame_strs.append(f'"duration": 1.0,\n"texture": ExtResource("{rid}")')
lines.append('}, {\n'.join(frame_strs))
lines.append('}],')
lines.append('"loop": true,')
lines.append('"name": &"default",')
lines.append('"speed": 30.0')
lines.append('}, {')

# attack 动画（速度 2 倍：40）
frame_strs = []
for rid in attack_ids:
    frame_strs.append(f'"duration": 1.0,\n"texture": ExtResource("{rid}")')
lines.append('"frames": [{')
lines.append('}, {\n'.join(frame_strs))
lines.append('}],')
lines.append('"loop": false,')
lines.append('"name": &"attack",')
lines.append('"speed": 40.0')
lines.append('}')

lines.append(']')

content = "\n".join(lines) + "\n"

out = os.path.join(ROOT, "scenes", "creature_visuals", "scout_frames.tres")
with open(out, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"Generated {out}")
print(f"idle frames: {len(idle_frames)}, attack frames: {len(attack_frames)}")
print(f"idle path: images/animation/scout_idle/")
print(f"attack path: images/animation/scout_attack/")
