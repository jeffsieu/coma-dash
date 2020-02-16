extends KinematicBody

const max_health = 100
const movement_speed = 2
const gravity = 5

var health = 100
var velocity = Vector3()

onready var player = $"/root/Level/Player"

func _ready():
	$HealthBar.set_max_health(health)
	$HealthBar.update_health(health)

func _physics_process(delta):
	var direction_to_player = (player.transform.origin - transform.origin).normalized()
	
	velocity.y = -gravity
	velocity = move_and_slide(movement_speed * direction_to_player)

func damage(bullet):
	health -= bullet.damage
	$HealthBar.update_health(health)
	if health <= 0:
		_die()

func _die():
	.queue_free()
