extends TouchScreenButton

var radius = Vector2(250, 250)
var parent_radius = 375
onready var parent_scale = $"..".scale
var joystick_direction = Vector2()

func _ready():
	pass # Replace with function body.

func reset_position():
	set_position(-radius)
	
func _input(event):
	if event is InputEventScreenDrag:
		var new_position = event.position
		var parent_position = $"..".global_position
		
		var angle = parent_position.angle_to(new_position)
		var direction_vector = new_position - parent_position
		direction_vector = direction_vector.clamped(parent_radius * parent_scale.x)
		new_position = parent_position + direction_vector
		new_position = new_position - radius * parent_scale
		joystick_direction = direction_vector.normalized()
		set_global_position(new_position)
	elif event is InputEventScreenTouch and not event.is_pressed():
		joystick_direction = Vector2()
# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
