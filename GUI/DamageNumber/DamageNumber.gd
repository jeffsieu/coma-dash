extends Sprite3D

const _LIFETIME := 1.0
const _SPEED := 5

var _damage: int
var _direction = Vector3(1, 0, 1)
var _total_delta: float = 0
var _color: Color

onready var _label = $Label

func _ready() -> void:
	var entity = get_parent()

func set_damage(damage: int):
	_damage = damage
	$Label.text = str(damage)

func set_damage_effectiveness(effectiveness: float) -> void:
	if effectiveness < 0.5:
		_color = Color.green
	elif effectiveness < 0.75:
		_color = Color.orange
	else:
		_color = Color.red
	$Label.modulate = _color

func _physics_process(delta: float) -> void:
	_total_delta += delta
	if _total_delta > _LIFETIME:
		queue_free()
	else:
		var progress = _total_delta / _LIFETIME
		var pos = get_global_transform().origin
		var cam = get_tree().get_root().get_camera()
		var screen_pos = cam.unproject_position(pos)
		var size = _label.get_size()
		var scale = _label.get_scale()
		var position = screen_pos + Vector2(0, -size.y * scale.y)
		_label.set_position(position)
		_label.modulate.a = 1.0 - progress
		translation += _direction * delta * _SPEED
