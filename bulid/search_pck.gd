extends SceneTree

# Usage: godot --headless --path . -s res://bulid/search_pck.gd <pck_path> <keyword>
func _init():
	var args = OS.get_cmdline_user_args()
	if args.size() < 2:
		print("Usage: search_pck.gd <pck_path> <keyword>")
		quit(1)
		return
	var pck_path = args[0]
	var keyword = args[1]
	var f = FileAccess.open(pck_path, FileAccess.READ)
	if f == null:
		print("Cannot open: ", pck_path)
		quit(1)
		return
	# Skip header: magic(4) + format(4) + major(4) + minor(4) + patch(4) + flags(4) + count(4) + optional key(32)
	var buf = f.get_buffer(4)
	var format_ver = f.get_32()
	var ver_major = f.get_32()
	var ver_minor = f.get_32()
	var ver_patch = f.get_32()
	var flags = f.get_32()
	var count = f.get_32()
	print("Godot ", ver_major, ".", ver_minor, ".", ver_patch, " files=", count)
	if flags & 1:
		f.get_buffer(32) # skip encryption key
	for i in range(count):
		var path_len = f.get_32()
		var path = f.get_buffer(path_len).get_string_from_utf8()
		var offset = f.get_64()
		var size = f.get_64()
		var md5 = f.get_buffer(16).hex_encode()
		var file_flags = f.get_32()
		if keyword.to_lower() in path.to_lower():
			print(path, "  (size=", size, ")")
	quit(0)
