extends SceneTree

# Usage: godot --headless --path . --script res://bulid/extract_pck_file.gd -- <pck_path> <keyword> <out_dir>
func _init():
	var args = OS.get_cmdline_user_args()
	if args.size() < 3:
		print("Usage: extract_pck_file.gd <pck_path> <keyword> <out_dir>")
		quit(1)
		return
	var pck_path = args[0]
	var keyword = args[1]
	var out_dir = args[2]
	var f = FileAccess.open(pck_path, FileAccess.READ)
	if f == null:
		print("Cannot open: ", pck_path)
		quit(1)
		return
	# Skip header: magic(4) + format(4) + major(4) + minor(4) + patch(4) + flags(4) + count(4) + optional key(32)
	f.get_buffer(4)
	f.get_32() # format
	f.get_32() # major
	f.get_32() # minor
	f.get_32() # patch
	var flags = f.get_32()
	var count = f.get_32()
	if flags & 1:
		f.get_buffer(32) # skip encryption key
	for i in range(count):
		var path_len = f.get_32()
		if path_len <= 0 or path_len > 100000:
			print("BAD path_len at ", i)
			break
		var path = f.get_buffer(path_len).get_string_from_utf8()
		var offset = f.get_64()
		var size = f.get_64()
		f.get_buffer(16) # md5
		f.get_32() # file flags
		if keyword.to_lower() in path.to_lower():
			var old_pos = f.get_position()
			f.seek(offset)
			var content = f.get_buffer(size)
			f.seek(old_pos)
			var safe_name = path.replace("/", "_").replace("\\", "_").replace(":", "_")
			var out_path = out_dir + "/" + safe_name
			var out_file = FileAccess.open(out_path, FileAccess.WRITE)
			if out_file != null:
				out_file.store_buffer(content)
				out_file.close()
				print("Extracted: ", path, " -> ", out_path, " (", size, " bytes)")
			else:
				print("Cannot write: ", out_path)
	quit(0)
