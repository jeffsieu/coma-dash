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
var value: int

var _player

func _ready() -> void:
	$ItemArea.connect("body_entered", self, "_on_body_entered")

func set_value(value: int) -> void:
	self.value = value

func _physics_process(delta: float) -> void:
	if _is_flying_to_player:
		var distance_to_player = (_player.transform.origin - transform.origin).length()
		move_and_slide((_player.transform.origin - transform.origin).normalized() / pow(distance_to_player, 2) * 100)

func _on_body_entered(body: PhysicsBody) -> void:
	if body is Player and not _is_collected:
		emit_signal("collected", self)
		_die()

func fly_to(player) -> void:
	_is_flying_to_player = true
	self._player = player

func _die() -> void:
	_is_collected =  true
	_is_flying_to_player = false
	.queue_free()
