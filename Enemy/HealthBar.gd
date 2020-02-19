extends Sprite3D

const Enemy = preload("res://Enemy/Enemy.gd")

func _ready() -> void:
	var entity = $".."
	entity.connect("health_changed", self, "_on_health_changed")

func _physics_process(delta: float) -> void:
	var pos = get_global_transform().origin
	var cam = get_tree().get_root().get_camera()
	var screenPos = cam.unproject_position(pos)
	var size = $ProgressBar.get_size()
	var scale = $ProgressBar.get_scale()
	$ProgressBar.set_position(screenPos + Vector2(-size.x * scale.x / 2, -size.y * scale.y))

func _on_health_changed(entity: Enemy) -> void:
	$ProgressBar.set_max(entity.max_health)
	$ProgressBar.set_value(entity.health)
