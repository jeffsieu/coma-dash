extends Sprite

func _ready():
	visible = false
	
func _input(event):
	if event is InputEventScreenTouch and event.is_pressed():
		visible = true
		set_global_position(event.position)
		$JoystickKnob.reset_position()
		$"/root/Level/Player".is_shooting = true
	elif event is InputEventScreenTouch:
		visible = false
		$"/root/Level/Player".is_shooting = false
		$JoystickKnob.reset_position()
