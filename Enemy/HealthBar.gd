extends Sprite3D

const Enemy = preload("res://Enemy/Enemy.gd")

var _initialized := false

func _ready() -> void:
	var entity = $".."
	entity.connect("health_changed", self, "_on_health_changed")

func _physics_process(delta: float) -> void:
	var pos = get_global_transform().origin
	var cam = get_tree().get_root().get_camera()
	var screen_pos = cam.unproject_position(pos)
	var size = $ProgressBar.get_size()
	var scale = $ProgressBar.get_scale()
	$ProgressBar.set_position(screen_pos + Vector2(-size.x * scale.x / 2, -size.y * scale.y))

func _on_health_changed(entity: Enemy, old_health) -> void:
	$ProgressBar.set_max(entity.MAX_HEALTH)
	if not _initialized:
		$ProgressBar.set_value(entity.health)
		_initialized = true
	else:
		var tween := $Tween
		tween.interpolate_method($ProgressBar, "set_value", old_health, entity.health, 0.1, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
		tween.start()
