extends Camera

onready var _player: Player = get_tree().get_root().find_node("LevelManager", true, false).find_node("Player")
var _initialized := false
var _velocity: Vector3
const _BASE_ACCELERATION = 80
const _FRICTION = 0.9
const _CAMERA_HEIGHT = 25
const _THRESHOLD_DISTANCE = 2 # Maximum distance from the player at which the camera won't move

func _physics_process(delta: float) -> void:
	if not _initialized:
		transform.origin = _player.transform.origin
		_initialized = true
		return

	var player_origin := _player.transform.origin
	var target_origin = Vector3(player_origin.x, player_origin.y + _CAMERA_HEIGHT, player_origin.z)
	var direction = transform.origin.direction_to(target_origin)
	var distance_to_player = transform.origin.distance_to(target_origin) - _THRESHOLD_DISTANCE
	distance_to_player = max(distance_to_player, 0)

	var acceleration = _BASE_ACCELERATION * distance_to_player * direction

	_velocity += acceleration * delta
	_velocity *= _FRICTION
	transform.origin += _velocity * delta
