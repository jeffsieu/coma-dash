extends Sprite3D

const Enemy = preload("res://Enemy/Enemy.gd")

func _ready() -> void:
	texture = $Viewport.get_texture()
	var entity = $".."
	entity.connect("health_changed", self, "_on_health_changed")

func _on_health_changed(entity: Enemy) -> void:
	$Viewport/ProgressBar.set_max(entity.max_health)
	$Viewport/ProgressBar.set_value(entity.health)
