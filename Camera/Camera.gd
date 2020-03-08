extends Camera

onready var _player: Player = get_tree().get_root().find_node("LevelManager", true, false).find_node("Player")

var _initialized := false
var _velocity: Vector3
var _actual_position: Vector3
var _trauma := 0.0
var _shake_delta := 0.0

const _BASE_ACCELERATION = 80
const _FRICTION = 0.9
const _CAMERA_HEIGHT = 25
const _THRESHOLD_DISTANCE = 2 # Maximum distance from the player at which the camera won't move

const _MAX_TRAUMA = 1;
const _TRAUMA_FALL = 0.05;
const _ROTATION_SHAKE_SCALE = 5
const _X_SHAKE_SCALE = 1
const _Z_SHAKE_SCALE = 1
const _Y_ROTATION_BASE = -90
const _NOISE_PERIOD = 0.5

onready var noise := OpenSimplexNoise.new()

func _ready() -> void:
	noise.set_period(_NOISE_PERIOD)

func _physics_process(delta: float) -> void:
	if not _initialized:
		var player_origin := _player.transform.origin
		_actual_position = Vector3(player_origin.x, player_origin.y + _CAMERA_HEIGHT * 1.5,  player_origin.z) # Animate camera down from above
		transform.origin = _actual_position
		_initialized = true
		return

	var player_origin := _player.transform.origin
	var target_origin = Vector3(player_origin.x, player_origin.y + _CAMERA_HEIGHT, player_origin.z)
	var direction = _actual_position.direction_to(target_origin)
	var distance_to_player = _actual_position.distance_to(target_origin) - _THRESHOLD_DISTANCE
	distance_to_player = max(distance_to_player, 0)

	var acceleration = _BASE_ACCELERATION * distance_to_player * direction

	_velocity += acceleration * delta
	_velocity *= _FRICTION
	_actual_position += _velocity * delta

	_screen_shake_process(delta)

func _screen_shake_process(delta: float) -> void:
	_trauma = clamp(_trauma - _TRAUMA_FALL, 0, _MAX_TRAUMA)
	var shake := pow(_trauma, 2)
	var x := _actual_position.x
	var z := _actual_position.z
	transform.origin = _actual_position
	transform.origin.x += _X_SHAKE_SCALE * shake * noise.get_noise_2d(0, _shake_delta)
	transform.origin.z += _Z_SHAKE_SCALE * shake * noise.get_noise_2d(1, _shake_delta)
	rotation_degrees.y = _Y_ROTATION_BASE + _ROTATION_SHAKE_SCALE * shake * noise.get_noise_2d(2, _shake_delta)

	_shake_delta += delta
	if _trauma == 0:
		_shake_delta = 0

func apply_damage_trauma(effectiveness: float) -> void:
	_trauma += effectiveness
