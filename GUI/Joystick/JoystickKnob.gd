extends TouchScreenButton

onready var _parent_radius: float = get_parent().texture.get_height() / 2
onready var _parent: Sprite = get_parent()
var _size := self.normal.get_size()
var joystick_direction := Vector2()

func reset_position() -> void:
	set_position(-_size / 2)
	joystick_direction = Vector2.ZERO

func _input(event: InputEvent) -> void:
	if _parent.enabled:
		if event is InputEventScreenDrag:
			var new_position = event.position
			var parent_position = _parent.global_position
			var direction_vector = new_position - parent_position
			direction_vector = direction_vector.clamped(_parent_radius * _parent.scale.x)
			new_position = parent_position + direction_vector
			new_position = new_position - (_size / 2) * _parent.scale
			joystick_direction = direction_vector.normalized()
			set_global_position(new_position)
		elif event is InputEventScreenTouch and not event.is_pressed():
			joystick_direction = Vector2()
