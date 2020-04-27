extends Weapon

class_name TripleGun

const Bullet = preload("res://Bullet/Bullet.tscn")

const _SHOOT_INTERVAL := 0.1
var shoot_cooldown: float = 0
var deg_spread: float = 30.0

func _try_shoot(delta: float) -> bool:
	var has_shot := shoot_cooldown <= 0
	if has_shot:
		shoot_cooldown += _SHOOT_INTERVAL
		var bullet := Bullet.instance()
		bullet.translation = _player.translation + _player.transform.basis.z * 2.2
		bullet.rotation = _player.rotation
		var bullet1 := Bullet.instance()
		bullet1.translation = _player.translation + _player.transform.basis.z * 2.2
		bullet1.rotation_degrees = _player.rotation_degrees + _player.transform.basis.y * deg_spread
		var bullet2 := Bullet.instance()
		bullet2.translation = _player.translation + _player.transform.basis.z * 2.2
		bullet2.rotation_degrees = _player.rotation_degrees - _player.transform.basis.y * deg_spread
		_player._level_manager.add_child(bullet)
		_player._level_manager.add_child(bullet1)
		_player._level_manager.add_child(bullet2)
	shoot_cooldown -= delta
	return has_shot
