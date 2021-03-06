extends KinematicBody
class_name Crate

var health: int setget _health_set, _health_get

onready var _level_manager = get_tree().get_root().find_node("LevelManager", true, false)
onready var _player = _level_manager.find_node("Player")

signal health_changed
signal died

func _ready() -> void:
	add_to_group("crates")
	self.connect("died", _level_manager, "_on_crate_died")

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
		die()

func generate_drops() -> Array:
	push_error("generate_drops not implemented by %s" % filename)
	return []

func die() -> void:
	emit_signal("died", self)
	queue_free()
