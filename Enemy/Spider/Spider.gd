extends "res://Enemy/Enemy.gd"
class_name Spider

const Collectible = preload("res://Loot/Collectible.gd")
const Crystal := preload("res://Loot/Crystal/Crystal.tscn")
const ExpOrb = preload("res://Loot/ExpOrb/ExpOrb.tscn")

const MAX_HEALTH = 100
const MOVEMENT_SPEED := 2.5
const GRAVITY := 5
const DAMAGE := 30
const DROP_CHANCE := 1.0/2.0

var has_loot := false

func _ready() -> void:
	health = MAX_HEALTH
	velocity = Vector3()
	emit_signal("health_changed", self, health)
	
	var anim = find_node("AnimationPlayer").get_animation("Walking")
	anim.set_loop(true)
	find_node("AnimationPlayer").set_speed_scale(1.8)
	find_node("AnimationPlayer").play("Walking")

func _physics_process(delta: float) -> void:
	var direction_to_player = (_player.transform.origin - transform.origin).normalized()
	rotation.y = deg2rad(90) - Vector2(direction_to_player.x, direction_to_player.z).angle()
	velocity.y = -GRAVITY
	var collision = move_and_collide(MOVEMENT_SPEED * direction_to_player * delta)
	if collision and collision.collider is Player:
		_damage_player(collision.collider)

func _damage_player(player: Player) -> void:
	player.on_damaged_by(self)
	
func generate_drops() -> Array:
	var drops := Array()
	if randf() < DROP_CHANCE:
		var crystal := Crystal.instance()
		crystal.set_value(randi() % 5 + 1)
		drops.append(crystal)
		
	var expOrb := ExpOrb.instance()
	expOrb.set_value(randi() % 2 + 1)
	drops.append(expOrb)
	return drops
