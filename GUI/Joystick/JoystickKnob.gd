extends TouchScreenButton

onready var parent_radius:float = $"..".texture.get_height() / 2
onready var parent: Sprite = $".."
var size := self.normal.get_size()
var joystick_direction := Vector2()

func reset_position() -> void:
	set_position(-size / 2)
	
func _input(event: InputEvent) -> void:
	if event is InputEventScreenDrag:
		var new_position = event.position
		var parent_position = $"..".global_position
		var direction_vector = new_position - parent_position
		direction_vector = direction_vector.clamped(parent_radius * parent.scale.x)
		new_position = parent_position + direction_vector
		new_position = new_position - (size / 2) * parent.scale
		joystick_direction = direction_vector.normalized()
		set_global_position(new_position)
	elif event is InputEventScreenTouch and not event.is_pressed():
		joystick_direction = Vector2()
