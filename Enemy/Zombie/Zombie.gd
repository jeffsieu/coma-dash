extends "res://Enemy/Enemy.gd"
class_name Zombie

const Collectible = preload("res://Collectible/Collectible.gd")
const Crystal := preload("res://Collectible/Crystal/Crystal.tscn")
const ExpOrb = preload("res://Collectible/ExpOrb/ExpOrb.tscn")

const _DROP_CHANCE := 1.0/2.0
const _GRAVITY := 5
const _MOVEMENT_SPEED := 2

const DAMAGE := 30
const MAX_HEALTH = 100

func _ready() -> void:
	health = MAX_HEALTH
	velocity = Vector3()
	emit_signal("health_changed", self, health)

	var anim = find_node("AnimationPlayer").get_animation("Walking")
	anim.set_loop(true)
	find_node("AnimationPlayer").set_speed_scale(1)
	find_node("AnimationPlayer").play("Walking")

func _move(delta: float) -> void:
	velocity.y = -_GRAVITY
	var direction_to_player = transform.origin.direction_to(_player.transform.origin)
	var collision = move_and_collide(_MOVEMENT_SPEED * direction_to_player * delta)
	if collision and collision.collider is Player:
		_damage_player(collision.collider)

func _damage_player(player: Player) -> void:
	player.on_damaged_by(self)

func generate_drops() -> Array:
	var drops := Array()
	if randf() < _DROP_CHANCE:
		var crystal := Crystal.instance()
		crystal.value = randi() % 5 + 1
		drops.append(crystal)

	var expOrb := ExpOrb.instance()
	expOrb.value = randi() % 2 + 1
	drops.append(expOrb)
	return drops
