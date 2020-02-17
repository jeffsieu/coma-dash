extends "res://Enemy/Enemy.gd"
class_name Zombie

const movement_speed := 2
const gravity := 5
const damage := 30

var has_loot := false

onready var player = $"/root/Level/Player"

func _ready() -> void:
	max_health = 100
	health = max_health
	velocity = Vector3()
	add_to_group("enemies")
	emit_signal("health_changed", self)

func _physics_process(delta: float) -> void:
	var direction_to_player = (player.transform.origin - transform.origin).normalized()
	velocity.y = -gravity
	velocity = move_and_slide(movement_speed * direction_to_player)
