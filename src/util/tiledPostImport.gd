extends Node


func post_import(scene):
	var file_path := "res://src/map/".plus_file(scene.get_name() + ".tscn")
	var packed_scene := PackedScene.new()

	if packed_scene.pack(scene) == OK:
		ResourceSaver.save(file_path, packed_scene)
	else:
		printerr(scene.get_name() + "couldn't be saved to: " + file_path)

	return scene

func set_map_script(scene_path: String) -> void:
	var map: Node = load(scene_path).instance()
	var script_path: String

	match map.get_name():
		"zone_2":
			script_path = "res://src/map/Zone2.cs"
		_:
			script_path = "res://src/map/Map.cs"

	map.set_script(load(script_path))

	var packed_scene := PackedScene.new()
	packed_scene.pack(map)
	ResourceSaver.save(map.get_filename(), packed_scene)

