extends ColorRect

func set_map_script(scene_path: String) -> void:
	var map_scene = load(scene_path)
	var map = map_scene.instance()
	map.set_script(load("res://src/map/Map.cs"))
	var packed_scene = PackedScene.new()
	packed_scene.pack(map)
	ResourceSaver.save(map.get_filename(), packed_scene)
