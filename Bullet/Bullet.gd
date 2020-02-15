extends KinematicBody

var timer = 0
var decay_limit = 2
var speed = 100
var damage = 30

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.
	
func _process(delta):
	timer += delta
	if timer > decay_limit:
		free()

func _physics_process(delta):
	var velocity = transform.basis.z * speed
	var collision = move_and_collide(velocity * delta)
	if collision:
		if collision.collider.name == "Zombie":
			collision.collider.damage(self)
			queue_free()
