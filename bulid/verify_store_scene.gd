extends SceneTree

# Usage: godot --headless --path . -s res://bulid/verify_store_scene.gd
func _init():
	var scene = load("res://scenes/merchant/characters/scout_merchant.tscn")
	if scene == null:
		print("FAIL: scene is null")
		quit(1)
		return
	var inst = scene.instantiate()
	if inst == null:
		print("FAIL: instance is null")
		quit(1)
		return
	print("OK: instantiated type = ", inst.get_class())
	var sprite = inst.get_node_or_null("AnimatedSprite2D")
	if sprite == null:
		print("FAIL: AnimatedSprite2D not found")
		quit(1)
		return
	var frames = sprite.sprite_frames
	if frames == null:
		print("FAIL: sprite_frames is null")
		quit(1)
		return
	var anims = frames.get_animation_names()
	print("OK: animations = ", anims)
	if anims.size() == 0:
		print("FAIL: no animations")
		quit(1)
		return
	var frame_count = frames.get_frame_count("default")
	print("OK: default frame count = ", frame_count)
	print("OK: script class = ", inst.get_script())
	quit(0)
