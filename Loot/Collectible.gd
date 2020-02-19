extends KinematicBody
class_name Collectible

enum {
	CRYSTAL,
	EXPORB
}

var Player = load("res://Player/Player.gd")

signal collected

var _is_flying_to_player := false
var _is_collected := false
var velocity := Vector3()
var value: int

var player

func set_value(value: int) -> void:
	self.value = value

func _physics_process(delta: float) -> void:
	if _is_flying_to_player:
		var distance_to_player = (player.transform.origin - transform.origin).length()
		velocity = move_and_slide((player.transform.origin - transform.origin).normalized() / pow(distance_to_player, 2) * 100)
		for i in range(get_slide_count()):
			var collision = get_slide_collision(i)
			if collision.collider is Player and not _is_collected:
				emit_signal("collected", self)
				_die()

func fly_to(player) -> void:
	_is_flying_to_player = true
	self.player = player

func _die() -> void:
	_is_collected =  true
	.queue_free()
