extends KinematicBody
class_name Bullet

const _decay_delta := 2.0
const _speed := 100
const _damage := 30

var timer: float = 0.0
	
func _process(delta: float) -> void:
	timer += delta
	if timer > _decay_delta:
		queue_free()

func _physics_process(delta: float) -> void:
	var velocity := transform.basis.z * _speed
	var collision := move_and_collide(velocity * delta)
	if collision:
		if collision.collider.is_in_group("enemies"):
			collision.collider.on_damaged(_damage)
			queue_free()
