extends KinematicBody
class_name Enemy

var health: int setget _health_set, _health_get
var max_health: int
var velocity: Vector3

signal health_changed
signal died

func _health_set(new_health: int) -> void:
	if health != new_health:
		emit_signal("health_changed", self, health)
	health = new_health
	
func _health_get() -> int:
	return health

func on_damaged(damage: int) -> void:
	var old_health := health
	health -= damage
	emit_signal("health_changed", self, old_health)
	if health <= 0:
		_die()
		
func generate_drops() -> Array:
	push_error("generate_drops not implemented by %s" % filename)
	return []
	
func _die() -> void:
	emit_signal("died", self)
	queue_free()
