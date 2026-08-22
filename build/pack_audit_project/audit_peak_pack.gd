extends SceneTree

const REQUIRED_SCENE_RESOURCES: Array[String] = [
	"res://scenes/creature_visuals/scout_frames.tres",
	"res://scenes/creature_visuals/scout.tscn",
]

const SCOUT_IDLE_FRAME_COUNT: int = 34
const SCOUT_DEATH_FRAME_COUNT: int = 23

const ENERGY_SCENE_PATH: String = \
	"res://scenes/combat/energy_counters/scout_energy_counter.tscn"

const REQUIRED_ENERGY_NODES: Array[StringName] = [
	&"Layers",
	&"RotationLayers",
	&"EnergyVfxBack",
	&"EnergyVfxFront",
]


## 挂载指定的 Peak PCK，并强制加载运行时关键资源与能量场景契约。
func _init() -> void:
	var arguments: PackedStringArray = OS.get_cmdline_user_args()
	if arguments.size() != 1:
		_fail("用法：--script res://audit_peak_pack.gd -- <peak.pck>")
		return

	var pack_path: String = arguments[0]
	if not FileAccess.file_exists(pack_path):
		_fail("PCK 不存在：%s" % pack_path)
		return

	if not ProjectSettings.load_resource_pack(pack_path, true):
		_fail("无法挂载 PCK：%s" % pack_path)
		return

	var required_resources: Array[String] = _get_required_resources()
	var failures: Array[String] = []
	for resource_path: String in required_resources:
		var resource: Resource = ResourceLoader.load(
			resource_path,
			"",
			ResourceLoader.CACHE_MODE_IGNORE,
		)
		if resource == null:
			failures.append("无法加载关键资源：%s" % resource_path)

	_audit_energy_scene(failures)

	if not failures.is_empty():
		for failure: String in failures:
			push_error("[PeakPackAudit] %s" % failure)
		quit(1)
		return

	print_rich(
		"[color=green][PeakPackAudit] PASS[/color] pack=%s resources=%d energy_contract=ok"
		% [pack_path, required_resources.size()]
	)
	quit(0)


## 返回角色场景以及所有待机、死亡动画帧，避免抽样检查漏掉局部损坏。
func _get_required_resources() -> Array[String]:
	var resources: Array[String] = REQUIRED_SCENE_RESOURCES.duplicate()
	for frame_index: int in SCOUT_IDLE_FRAME_COUNT:
		resources.append(
			"res://images/animation/scout_idle/skeleton_scout_%02d.png"
			% frame_index
		)
	for frame_index: int in SCOUT_DEATH_FRAME_COUNT:
		resources.append(
			"res://images/animation/death/skeleton-death_%02d.png"
			% frame_index
		)
	return resources


## 检查自定义能量计数器是否满足原版 [code]NEnergyCounter[/code] 的节点与字体契约。
func _audit_energy_scene(failures: Array[String]) -> void:
	var resource: Resource = ResourceLoader.load(
		ENERGY_SCENE_PATH,
		"",
		ResourceLoader.CACHE_MODE_IGNORE,
	)
	var packed_scene: PackedScene = resource as PackedScene
	if packed_scene == null:
		failures.append("无法加载能量场景：%s" % ENERGY_SCENE_PATH)
		return

	var state: SceneState = packed_scene.get_state()
	var found_nodes: Dictionary[StringName, bool] = {}
	var label_has_font_override: bool = false

	for node_index: int in state.get_node_count():
		var node_name: StringName = state.get_node_name(node_index)
		if node_name in REQUIRED_ENERGY_NODES:
			found_nodes[node_name] = true
		if node_name == &"Label":
			label_has_font_override = _node_has_property(
				state,
				node_index,
				&"theme_override_fonts/font",
			)

	for node_name: StringName in REQUIRED_ENERGY_NODES:
		if not found_nodes.has(node_name):
			failures.append("能量场景缺少必需节点：%%%s" % node_name)

	if not label_has_font_override:
		failures.append("能量场景 Label 缺少 theme_override_fonts/font")


## 判断序列化场景中的指定节点是否显式保存了某个属性。
func _node_has_property(
	state: SceneState,
	node_index: int,
	property_name: StringName,
) -> bool:
	for property_index: int in state.get_node_property_count(node_index):
		if state.get_node_property_name(node_index, property_index) == property_name:
			return true
	return false


## 输出不可忽略的审计错误并以失败码退出。
func _fail(message: String) -> void:
	push_error("[PeakPackAudit] %s" % message)
	quit(2)
