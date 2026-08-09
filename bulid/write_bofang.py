content = b"using Godot;\n\npublic partial class bofang : AnimatedSprite2D\n{\n\tpublic override void _Ready()\n\t{\n\t\tPlay(\"default\");\n\t}\n}\n"
with open(r'C:\Users\shiqian\Peak\scenes\creature_visuals\bofang.cs', 'wb') as f:
    f.write(content)
print('written')