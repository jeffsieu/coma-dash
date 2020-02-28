extends "res://Enemy/Enemy.gd"
class_name Zombie

const Collectible = preload("res://Loot/Collectible.gd")
const Crystal := preload("res://Loot/Crystal.tscn")
const ExpOrb = preload("res://Loot/ExpOrb.tscn")

const movement_speed := 2
const gravity := 5
const damage := 30
const drop_chance := 1.0/2.0

var has_loot := false

func _ready() -> void:
	max_health = 100
	health = max_health
	velocity = Vector3()
	emit_signal("health_changed", self, health)

func _physics_process(delta: float) -> void:
	var direction_to_player = (_player.transform.origin - transform.origin).normalized()
	rotation.y = deg2rad(90) - Vector2(direction_to_player.x, direction_to_player.z).angle()
	velocity.y = -gravity
	var collision = move_and_collide(movement_speed * direction_to_player * delta)
	if collision and collision.collider is Player:
		_damage_player(collision.collider)

func _damage_player(player: Player) -> void:
	player.on_damaged_by(self)
	
func generate_drops() -> Array:
	var drops := Array()
	if randf() < drop_chance:
		var crystal := Crystal.instance()
		crystal.set_value(randi() % 5 + 1)
		drops.append(crystal)
		
	var expOrb := ExpOrb.instance()
	expOrb.set_value(randi() % 2 + 1)
	drops.append(expOrb)
	return drops