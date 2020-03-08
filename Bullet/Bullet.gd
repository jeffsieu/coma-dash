extends KinematicBody
class_name Bullet

const DamageNumber := preload("res://GUI/DamageNumber/DamageNumber.tscn")

const _DECAY_DELTA := 2.0
const _SPEED := 100
const _MEAN_DAMAGE := 30.0
const _SPREAD := 10.0
const _KNOCKBACK := 0.5

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
		var collider := collision.collider
		if collider.is_in_group("enemies") or collider.is_in_group("crates"):
			var damage = _get_damage()
			var effectiveness = (damage - (_MEAN_DAMAGE - _SPREAD / 2)) / _SPREAD

			if collider.is_in_group("enemies"):
				collider.on_damaged(damage, velocity.normalized() * _KNOCKBACK)
			else:
				collider.on_damaged(damage)

			_show_damage_number(collision.collider.global_transform.origin, damage, effectiveness)
			queue_free()

func _get_damage() -> int:
	var damage_float := _MEAN_DAMAGE + (randf() - 0.5) * _SPREAD
	return int(round(damage_float))

func _show_damage_number(position: Vector3, damage: int, effectiveness: float) -> void:
	var damage_number := DamageNumber.instance()
	var transform_offset := Vector3(randf() * damage_number.SPAWN_OFFSET_RANGE, 0, randf() * damage_number.SPAWN_OFFSET_RANGE)
	damage_number.set_damage(damage)
	damage_number.set_damage_effectiveness(effectiveness)
	damage_number.global_transform.origin = position + transform_offset
	_level_manager.add_child(damage_number)
