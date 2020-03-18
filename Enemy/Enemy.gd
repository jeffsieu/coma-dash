extends KinematicBody
class_name Enemy

var health: int setget _health_set, _health_get
var velocity: Vector3
var _active := false

onready var _level_manager = get_tree().get_root().find_node("LevelManager", true, false)
onready var _player = _level_manager.find_node("Player")
onready var _tween: Tween = find_node("Tween")
onready var _health_bar: Sprite3D = find_node("HealthBar")
onready var _collider: CollisionShape = find_node("Collider")
onready var _animation_player: AnimationPlayer = find_node("Mesh").find_node("AnimationPlayer")

const _TWEEN_DURATION = 1.2

signal health_changed
signal died

func _ready() -> void:
	_spawn()
	_animation_player.playback_active = false

func _physics_process(delta: float) -> void:
	_face_player()
	if _active:
		_move(delta)

func _move(delta: float) -> void:
	push_error("_move not implemented by %s" % filename)

func _face_player() -> void:
	var direction_to_player = transform.origin.direction_to(_player.transform.origin)
	rotation.y = -Vector2(direction_to_player.x, direction_to_player.z).angle()

func _health_set(new_health: int) -> void:
	if health != new_health:
		emit_signal("health_changed", self, health)
	health = new_health
func _health_get() -> int:
	return health

func on_damaged(damage: int, knockback: Vector3) -> void:
	transform.origin += knockback

	var old_health := health
	health -= damage

	emit_signal("health_changed", self, old_health)
	if health <= 0:
		_die()

func generate_drops() -> Array:
	push_error("generate_drops not implemented by %s" % filename)
	return []

func _spawn() -> void:
	_health_bar.hide()
	_collider.disabled = true

	# placeholder for now, when we got spawn animations we can replace this with that
	transform.origin.y -= 2
	var origin = transform.origin
	var target_origin = origin
	target_origin.y += 2

	_tween.interpolate_property(self, "translation", origin, target_origin, _TWEEN_DURATION, Tween.TRANS_QUAD, Tween.EASE_OUT)
	_tween.interpolate_callback(self, _TWEEN_DURATION, "_activate")
	_tween.start()

func _die() -> void:
	emit_signal("died", self)
	# placeholder for now, when we got death animations we can replace this with that
	var origin = transform.origin
	var target_origin = origin
	target_origin.y -= 2

	_tween.interpolate_property(self, "translation", origin, target_origin, _TWEEN_DURATION, Tween.TRANS_QUAD, Tween.EASE_OUT)
	_tween.interpolate_callback(self, _TWEEN_DURATION, "_on_death_animation_finished")
	_tween.start()

	_deactivate()

func _on_death_animation_finished() -> void:
	queue_free()

func _activate() -> void:
	_active = true
	_health_bar.show()
	_collider.disabled = false
	_animation_player.playback_active = true
	add_to_group("enemies")

func _deactivate() -> void:
	_active = false

	_health_bar.hide()
	_collider.disabled = true
	_animation_player.playback_active = false
