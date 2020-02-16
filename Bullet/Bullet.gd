extends KinematicBody

const decay_limit = 2
const speed = 100
const damage = 30

var timer = 0

const Zombie = preload("res://Zombie/Zombie.gd")

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
		if collision.collider is Zombie:
			collision.collider.damage(self)
			queue_free()
