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
var _acceleration: int
var value: int
var velocity: Vector3

const DAMPING_FACTOR = 0.01
const _BASE_DISTANCE = 5
const _BASE_ACCELERATION = 50
const _FAST_FLY_BOOST_FACTOR = 20

var _player

func _ready() -> void:
	$ItemArea.connect("body_entered", self, "_on_body_entered")
	var initial_fly_direction = randf() * PI * 2
	velocity = Vector3(cos(initial_fly_direction) * 15, 2, sin(initial_fly_direction) * 15)

func set_value(value: int) -> void:
	self.value = value

func _physics_process(delta: float) -> void:
	if _is_flying_to_player:
		var distance_squared := transform.origin.distance_squared_to(_player.transform.origin)
		var direction := transform.origin.direction_to(_player.transform.origin)
		var acceleration := _acceleration * direction / distance_squared

		velocity += acceleration

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
	var distance := transform.origin.distance_to(player.transform.origin)
	_is_flying_to_player = true
	_acceleration = _BASE_ACCELERATION * pow(distance / _BASE_DISTANCE, 1.8)
	_player = player

func _die() -> void:
	_is_collected =  true
	_is_flying_to_player = false
	.queue_free()
