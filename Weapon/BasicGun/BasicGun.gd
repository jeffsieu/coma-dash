extends Weapon

class_name BasicGun

const Bullet = preload("res://Bullet/Bullet.tscn")

const _SHOOT_INTERVAL := 0.1
var shoot_cooldown: float = 0

func _try_shoot(delta: float) -> bool:
	var has_shot := shoot_cooldown <= 0
	if has_shot:
		shoot_cooldown += _SHOOT_INTERVAL
		var bullet := Bullet.instance()
		bullet.translation = _player.translation + _player.transform.basis.z * 2.2
		bullet.rotation = _player.rotation
		_player._level_manager.add_child(bullet)
	shoot_cooldown -= delta
	return has_shot
