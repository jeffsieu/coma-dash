extends Sprite3D

const _LIFETIME := 0.75
const _SPEED := 5

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
	_effectiveness = effectiveness
	$Label.modulate = _color

func _physics_process(delta: float) -> void:
	_total_delta += delta
	if _total_delta > _LIFETIME:
		queue_free()
	else:
		_update(delta)

func _update(delta: float) -> void:
	var progress = _total_delta / _LIFETIME
	var pos = get_global_transform().origin
	var cam = get_tree().get_root().get_camera()
	var screen_pos = cam.unproject_position(pos)
	var position = screen_pos - Vector2(_label.rect_size.x * 0.5, _label.rect_size.y * 0.75)
	
	var progress_scale_factor = 1.0 - progress / 2
	var effectiveness_scale_factor = 0.5 + 0.5 * _effectiveness
	var scale_factor = progress_scale_factor * effectiveness_scale_factor
	
	_label.set_position(position)
	_label.modulate.a = 1.0 - progress
	_label.rect_scale = Vector2(scale_factor, scale_factor)
	translation += _direction * delta * _SPEED
