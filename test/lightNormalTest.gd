extends Node2D

onready var camera2D: Camera2D = $map/camera2D
onready var tween: Tween = $tween


func _unhandled_input(event):
	if Input.is_action_just_pressed("ui_click"):
		tween.interpolate_property(camera2D, "global_position",
			camera2D.global_position, get_global_mouse_position(),
			1.0, Tween.TRANS_LINEAR, Tween.EASE_IN)
		tween.start()

