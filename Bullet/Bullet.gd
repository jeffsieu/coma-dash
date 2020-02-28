extends KinematicBody
class_name Bullet

const _DECAY_DELTA := 2.0
const _SPEED := 100
const _DAMAGE := 30

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
			collision.collider.on_damaged(_DAMAGE)
			queue_free()
