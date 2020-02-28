extends Camera

onready var _player: Player = get_tree().get_root().find_node("Level", true, false).get_node("Player")
var _initialized := false
var _velocity: Vector3
const _BASE_ACCELERATION = 80
const _FRICTION = 0.9
const _THRESHOLD_DISTANCE = 2 # Maximum distance from the player at which the camera won't move

func _physics_process(delta: float) -> void:
	if not _initialized:
		transform.origin.x = _player.transform.origin.x
		transform.origin.z = _player.transform.origin.z
		_initialized = true
		return
	var player_origin = Vector3(_player.transform.origin.x, 0, _player.transform.origin.z)
	var camera_origin = Vector3(transform.origin.x, 0, transform.origin.z)
	var direction = camera_origin.direction_to(player_origin)
	var distance_to_player = camera_origin.distance_to(player_origin) - _THRESHOLD_DISTANCE
	distance_to_player = max(distance_to_player, 0)

	var acceleration = _BASE_ACCELERATION * distance_to_player * direction

	_velocity += acceleration * delta
	_velocity *= _FRICTION
	transform.origin += _velocity * delta
