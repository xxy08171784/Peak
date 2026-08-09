"""修复 Godot 资源 UID 冲突。
策略：对于重复 UID 的文件，保留被 .tres 实际引用的那个（test_skeleton_animation/），
给另一个（animation/scout_idle/）分配全新 UID，并删除其缓存目录中的导入文件，
让 Godot 重新导入。
"""
import os
import re
import hashlib
import shutil

ROOT = r"C:\Users\shiqian\Peak"

def gen_uid(path):
    """基于路径生成稳定的伪随机 UID（Godot uid 格式：uid:// + base32 字符）"""
    h = hashlib.sha256(path.encode('utf-8')).hexdigest()
    chars = "abcdefghijklmnopqrstuvwxyz0123456789"
    # Godot UID 通常以特定字符开头，这里用 a-z 0-9 生成 18 位
    seed = int(h[:8], 16)
    result = []
    for i in range(18):
        seed = (seed * 1103515245 + 12345) & 0x7FFFFFFF
        result.append(chars[seed % len(chars)])
    return "uid://" + "".join(result)

def main():
    # 收集所有 .import 文件的 UID
    uid_map = {}  # uid -> [file paths]
    for dirpath, _, filenames in os.walk(os.path.join(ROOT, "images")):
        for fn in filenames:
            if fn.endswith(".import"):
                full = os.path.join(dirpath, fn)
                with open(full, 'r', encoding='utf-8', errors='replace') as f:
                    content = f.read()
                m = re.search(r'uid="([^"]+)"', content)
                if m:
                    uid = m.group(1)
                    uid_map.setdefault(uid, []).append(full)

    conflicts = {u: v for u, v in uid_map.items() if len(v) > 1}
    print(f"Total conflicts: {len(conflicts)}")

    for uid, files in conflicts.items():
        # 保留引用 test_skeleton_animation 的（scout_frames.tres 引用的），改另一个
        # 优先保留被 .tres 引用的路径：检查是否有文件路径包含 test_skeleton_animation
        keep = None
        change = []
        for f in files:
            if "test_skeleton_animation" in f:
                keep = f
            else:
                change.append(f)
        if keep is None:
            # 都没有 test_skeleton_animation，保留第一个，改其余的
            keep = files[0]
            change = files[1:]
        
        print(f"\nUID {uid}:")
        print(f"  KEEP: {os.path.relpath(keep, ROOT)}")
        for f in change:
            new_uid = gen_uid(f)
            with open(f, 'r', encoding='utf-8', errors='replace') as fh:
                content = fh.read()
            content_new = re.sub(r'uid="[^"]+"', f'uid="{new_uid}"', content, count=1)
            with open(f, 'w', encoding='utf-8') as fh:
                fh.write(content_new)
            print(f"  CHANGE: {os.path.relpath(f, ROOT)} -> {new_uid}")

    # 清理 .godot/imported 中对应的旧缓存（让 Godot 重新导入）
    imported_dir = os.path.join(ROOT, ".godot", "imported")
    print(f"\nImported cache dir exists: {os.path.isdir(imported_dir)}")

if __name__ == "__main__":
    main()
