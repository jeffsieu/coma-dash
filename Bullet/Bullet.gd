extends KinematicBody
class_name Bullet

const DamageNumber := preload("res://GUI/DamageNumber/DamageNumber.tscn")

const _DECAY_DELTA := 2.0
const _SPEED := 100
const _MEAN_DAMAGE := 30.0
const _SPREAD := 10.0

onready var _level_manager = get_tree().get_root().find_node("LevelManager", true, false)
var timer: float = 0.0

func _process(delta: float) -> void:
	timer += delta
	if timer > _DECAY_DELTA:
		queue_free()

func _physics_process(delta: float) -> void:
	var velocity := transform.basis.z * _SPEED
	var collision := move_and_collide(velocity * delta)
	if collision:
		if collision.collider.is_in_group("enemies") or collision.collider.is_in_group("crates"):
			var damage = _get_damage()
			collision.collider.on_damaged(damage)
			var effectiveness = (damage - (_MEAN_DAMAGE - _SPREAD / 2)) / _SPREAD
			_show_damage_number(damage, effectiveness)
			queue_free()

func _get_damage() -> int:
	var lower_bound = _MEAN_DAMAGE - _SPREAD / 2
	var upper_bound = _MEAN_DAMAGE + _SPREAD / 2
	var damage_float := randf() * _SPREAD + _MEAN_DAMAGE - _SPREAD / 2
	return int(round(damage_float))

func _show_damage_number(damage: int, effectiveness: float) -> void:
	var damage_number := DamageNumber.instance()
	damage_number.set_damage(damage)
	damage_number.set_damage_effectiveness(effectiveness)
	damage_number.global_transform.origin = global_transform.origin
	_level_manager.add_child(damage_number)
