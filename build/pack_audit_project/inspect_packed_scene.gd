extends SceneTree


## 挂载资源包并打印指定 [code]PackedScene[/code] 的序列化节点与属性。
func _init() -> void:
	var arguments: PackedStringArray = OS.get_cmdline_user_args()
	if arguments.size() < 2 or arguments.size() > 3:
		push_error(
			"用法：--script res://inspect_packed_scene.gd -- <pack.pck> <scene_path> [output.tscn]"
		)
		quit(2)
		return

	if not ProjectSettings.load_resource_pack(arguments[0], true):
		push_error("无法挂载资源包：%s" % arguments[0])
		quit(3)
		return

	var resource: Resource = ResourceLoader.load(
		arguments[1],
		"PackedScene",
		ResourceLoader.CACHE_MODE_IGNORE,
	)
	var packed_scene: PackedScene = resource as PackedScene
	if packed_scene == null:
		push_error("无法加载场景：%s" % arguments[1])
		quit(4)
		return

	var state: SceneState = packed_scene.get_state()
	if arguments.size() == 3:
		var save_result: Error = ResourceSaver.save(packed_scene, arguments[2])
		if save_result != OK:
			push_error("无法保存场景副本：%s error=%d" % [arguments[2], save_result])
			quit(5)
			return

	for node_index: int in state.get_node_count():
		var node_instance: PackedScene = state.get_node_instance(node_index)
		var instance_path: String = ""
		if node_instance != null:
			instance_path = node_instance.resource_path
		print(
			"NODE index=%d path=%s name=%s type=%s instance=%s"
			% [
				node_index,
				state.get_node_path(node_index),
				state.get_node_name(node_index),
				state.get_node_type(node_index),
				instance_path,
			]
		)
		for property_index: int in state.get_node_property_count(node_index):
			var property_name: StringName = state.get_node_property_name(
				node_index,
				property_index,
			)
			var property_value: Variant = state.get_node_property_value(
				node_index,
				property_index,
			)
			print(
				"  PROPERTY %s=%s"
				% [property_name, _describe_value(property_value)]
			)

	quit(0)


## 为资源属性补充类型与资源路径，避免只输出不稳定的运行时实例编号。
func _describe_value(value: Variant) -> String:
	if value is Resource:
		var resource: Resource = value as Resource
		var description: String = "%s path=%s" % [
			resource.get_class(),
			resource.resource_path,
		]
		if resource is FontVariation:
			var font_variation: FontVariation = resource as FontVariation
			var base_font: Font = font_variation.base_font
			if base_font != null:
				description += " base_font=%s" % base_font.resource_path
		return description
	return str(value)
