extends Sprite3D

const _LIFETIME := 1.25
const _APPEAR_DURATION := 0.1
const _DISAPPEAR_DURATION := 0.25
const _SPEED := 3.0
const SPAWN_OFFSET_RANGE := 1.0

var _damage: int
var _effectiveness: float
var _direction = Vector3(1, 0, 0)
var _total_delta: float = 0
var _color: Color

onready var _label = $Label

func _ready() -> void:
	$Label.rect_pivot_offset = $Label.rect_size / 2
	_update(0)

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

	_effectiveness = min(effectiveness, 1.2)
	$Label.modulate = _color

func _physics_process(delta: float) -> void:
	_total_delta += delta
	if _total_delta > _LIFETIME:
		queue_free()
	else:
		_update(delta)

func _update(delta: float) -> void:
	var pos = get_global_transform().origin
	var cam = get_tree().get_root().get_camera()
	var screen_pos = cam.unproject_position(pos)
	var position = screen_pos - Vector2(_label.rect_size.x * 0.5, _label.rect_size.y * 0.75)
	
	var time_scale_factor = clamp(_total_delta / _APPEAR_DURATION, 0, 1)
	var effectiveness_scale_factor = 0.5 + 0.5 * pow(_effectiveness, 2)
	var scale_factor = time_scale_factor * effectiveness_scale_factor
	
	var time_left = _LIFETIME - _total_delta
	var opacity = clamp(time_left / _DISAPPEAR_DURATION, 0, 1)
	_label.set_position(position)
	_label.modulate.a = opacity
	_label.rect_scale = Vector2(scale_factor, scale_factor)
	translation += _direction * delta * _SPEED
