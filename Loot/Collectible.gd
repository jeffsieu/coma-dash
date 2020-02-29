extends KinematicBody
class_name Collectible

enum {
	CRYSTAL,
	EXPORB,
	HEAL
}

var Player = load("res://Player/Player.gd")

signal collected

var _is_flying_to_player := false
var _is_collected := false
var _animation_delta = 0
var value: int
var velocity: Vector3

const DAMPING_FACTOR = 0.01

var _player

func _ready() -> void:
	$ItemArea.connect("body_entered", self, "_on_body_entered")
	var initial_fly_direction = randf() * PI * 2
	velocity = Vector3(cos(initial_fly_direction) * 15, 2, sin(initial_fly_direction) * 15)

func set_value(value: int) -> void:
	self.value = value

func _physics_process(delta: float) -> void:
	if _is_flying_to_player:
		var distance_to_player = (_player.transform.origin - transform.origin).length()
		velocity += (_player.transform.origin - transform.origin).normalized() / pow(distance_to_player, 2) * 50
	velocity *= pow(DAMPING_FACTOR, delta)
	move_and_slide(velocity)
	_animate_hover(delta)

func _animate_hover(delta: float) -> void:
	_animation_delta = fmod(_animation_delta + delta, deg2rad(360))
	var vertical_offset = sin(_animation_delta) -  sin(_animation_delta - delta)
	rotate_y(delta)
	translation.y += vertical_offset

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
