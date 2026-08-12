extends SceneTree

func _init():
	var f = FileAccess.open("res://localization/zhs/cards.json", FileAccess.READ)
	if f == null:
		print("CANNOT OPEN")
		quit(1)
		return
	var text = f.get_as_text()
	var json = JSON.new()
	var err = json.parse(text)
	if err != OK:
		print("JSON ERROR at line ", json.get_error_line(), ": ", json.get_error_message())
		quit(1)
		return
	var data = json.data
	print("JSON OK, top-level type: ", typeof(data))
	print("Has OVERRIDE: ", data.has("OVERRIDE.description"))
	print("Has ARSON_EXPERT: ", data.has("ARSON_EXPERT.description"))
	print("Has SNOWBALL_ROLLING: ", data.has("SNOWBALL_ROLLING.description"))
	print("Has WHERE_DREAMS_BEGIN: ", data.has("WHERE_DREAMS_BEGIN.description"))
	print("Has BANANA_PEEL: ", data.has("BANANA_PEEL.description"))
	print("OVERRIDE desc: ", data.get("OVERRIDE.description", "MISSING"))
	print("ARSON_EXPERT desc: ", data.get("ARSON_EXPERT.description", "MISSING"))
	print("SNOWBALL_ROLLING desc: ", data.get("SNOWBALL_ROLLING.description", "MISSING"))
	print("WHERE_DREAMS_BEGIN desc: ", data.get("WHERE_DREAMS_BEGIN.description", "MISSING"))
	print("BANANA_PEEL desc: ", data.get("BANANA_PEEL.description", "MISSING"))
	quit(0)
