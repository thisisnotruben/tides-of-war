tool
extends Light2D

export var speed := 1.0

export(Gradient) var gradient := preload("res://asset/img/light/gradient/fireLight.tres")
var current_index := 0
var direction := 1
var tween := Tween.new()


func _ready():
	add_child(tween)
	tween.connect("tween_completed", self, "_on_tween_completed")
	start_tween()

func _on_tween_completed(object: Object, key: NodePath):
	start_tween()

func start_tween():
	var next := (current_index + direction) % gradient.colors.size()
	if next == 0:
		direction *= -1

	printt(current_index, next, gradient.colors.size())
	tween.interpolate_property(self, "color",
		gradient.colors[current_index], gradient.colors[next],
		speed, Tween.TRANS_LINEAR, Tween.EASE_IN)
	tween.start()

	current_index = next

