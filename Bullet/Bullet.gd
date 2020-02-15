extends KinematicBody

var timer = 0
var decay_limit = 2
var speed = 1000

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.
	
func _process(delta):
	timer += delta
	if timer > decay_limit:
		free()

func _physics_process(delta):
	var velocity = transform.basis.z * speed
	velocity = move_and_slide(velocity * delta)
