extends ColorRect


func set_map_script(scene_path: String) -> void:
	var map: Node = load(scene_path).instance()

	var script_path := "res://src/map/Map.cs"
	if map.get_name() == "zone_2":
		script_path = "res://src/map/Zone2.cs"
	map.set_script(load(script_path))

	var explore_music := [""]
	match map.get_name():
		# TODO: would like each zone to have two tracks for exploring
		"zone_1", "zone_2", "zone_3", "zone_5":
			explore_music = ["zone_1_world_0", "zone_1_world_1"]
		"zone_4":
			explore_music = ["zone_4_world"]
	map.exploreMusic = explore_music

	var packed_scene := PackedScene.new()
	packed_scene.pack(map)
	ResourceSaver.save(map.get_filename(), packed_scene)
