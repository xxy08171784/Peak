extends SceneTree


## 从指定 PCK 原样提取一个内部文件，用于比较不同构建的二进制资源。
func _init() -> void:
	var arguments: PackedStringArray = OS.get_cmdline_user_args()
	if arguments.size() != 3:
		_fail("用法：--script res://extract_pack_file.gd -- <pack.pck> <res://path> <output>")
		return

	var pack_path: String = arguments[0]
	var resource_path: String = arguments[1]
	var output_path: String = arguments[2]
	if not ProjectSettings.load_resource_pack(pack_path, true):
		_fail("无法挂载 PCK：%s" % pack_path)
		return

	var source_file: FileAccess = FileAccess.open(resource_path, FileAccess.READ)
	if source_file == null:
		_fail("无法读取包内文件：%s error=%d" % [resource_path, FileAccess.get_open_error()])
		return

	var content: PackedByteArray = source_file.get_buffer(source_file.get_length())
	var output_file: FileAccess = FileAccess.open(output_path, FileAccess.WRITE)
	if output_file == null:
		_fail("无法创建输出文件：%s error=%d" % [output_path, FileAccess.get_open_error()])
		return
	output_file.store_buffer(content)
	print_rich(
		"[color=green][PackExtract] PASS[/color] resource=%s bytes=%d output=%s"
		% [resource_path, content.size(), output_path]
	)
	quit(0)


## 输出不可忽略的提取错误并以失败码退出。
func _fail(message: String) -> void:
	push_error("[PackExtract] %s" % message)
	quit(2)
