extends KinematicBody
class_name Enemy

var health: int setget _health_set, _health_get
var max_health: int
var velocity: Vector3

signal health_changed
signal died

func _health_set(health_new: int) -> void:
	if health != health_new:
		emit_signal("health_changed", self)
	health = health_new
	
func _health_get() -> int:
	return health

func on_damaged(damage: int) -> void:
	health -= damage
	emit_signal("health_changed", self)
	if health <= 0:
		_die()
		
func generate_drops() -> Array:
	push_error("generate_drops not implemented by %s" % filename)
	return []
	
func _die() -> void:
	emit_signal("died", self)
	queue_free()
