extends KinematicBody

var health = 100
var movement_speed = 2
var velocity = Vector3()
var gravity = 5

func _physics_process(delta):
	var direction_to_player = ($"/root/Level/Player".transform.origin - transform.origin).normalized()
	
	velocity.y = -gravity
	velocity = move_and_slide(movement_speed * direction_to_player)

func damage(bullet):
	print('im dmaged')
	health -= bullet.damage
	if health <= 0:
		_die()

func _die():
	.queue_free()
