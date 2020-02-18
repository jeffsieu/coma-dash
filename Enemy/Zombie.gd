extends "res://Enemy/Enemy.gd"
class_name Zombie

var _level

const movement_speed := 2
const gravity := 5
const damage := 30

var has_loot := false

var _player

func _ready() -> void:
	_level = get_tree().get_root().find_node("Level", true, false)
	_player = _level.get_node("Player")
	max_health = 100
	health = max_health
	velocity = Vector3()
	add_to_group("enemies")
	emit_signal("health_changed", self)

func _physics_process(delta: float) -> void:
	var direction_to_player = (_player.transform.origin - transform.origin).normalized()
	velocity.y = -gravity
	var collision = move_and_collide(movement_speed * direction_to_player * delta)
	if collision and collision.collider is Player:
		_damage_player(collision.collider)

func _damage_player(player: Player) -> void:
	player.on_damaged_by(self)
