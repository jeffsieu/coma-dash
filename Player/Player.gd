extends KinematicBody
class_name Player

const Bullet = preload("res://Bullet/Bullet.tscn")

onready var _level_manager = get_tree().get_root().find_node("LevelManager", true, false)

var _floor: Spatial

const _MOVEMENT_SPEED := 2.5
const _GRAVITY := 5
const _DAMPING_FACTOR := 0.6
const _FALL_THRESHOLD_DISTANCE = 2

const _SHOOT_INTERVAL := 0.1
var shoot_cooldown: float = 0
var is_shooting := false

const _DAMAGE_INTERVAL = 0.5
var damage_cooldown: float = _DAMAGE_INTERVAL

const MAX_HEALTH := 200
var health: int = MAX_HEALTH

var _velocity: Vector3 = Vector3()

signal health_changed
signal fell_off

func _ready() -> void:
	$ItemCollector.connect("area_entered", self, "_on_item_near")
	_level_manager.connect("level_loaded", self, "_on_level_loaded")
	_level_manager.connect("level_cleared", self, "_on_level_cleared")

func _get_joystick_direction() -> Vector3:
	return _level_manager.find_node("JoystickKnob").joystick_direction

func _try_shoot(delta: float) -> bool:
	var has_shot := shoot_cooldown <= 0
	if has_shot:
		shoot_cooldown += _SHOOT_INTERVAL
		var bullet := Bullet.instance()
		bullet.translation = translation + transform.basis.z * 2.2
		bullet.rotation = rotation
		_level_manager.add_child(bullet)
	shoot_cooldown -= delta
	return has_shot

func _rotate_body() -> void:
	var direction = _get_joystick_direction()
	if direction != Vector2():
		rotation.y = -direction.angle()

func _on_item_near(item: Area) -> void:
	if item and item.get_parent() is Collectible:
		item.get_parent().fly_to(self)

func _on_level_loaded(_level: Level) -> void:
	_floor = get_tree().get_root().find_node("Floor", true, false)

func _on_level_cleared(_level: Level) -> void:
	_floor = null

func _process(delta: float) -> void:
	_rotate_body()

func _physics_process(delta: float) -> void:
	if is_shooting:
		var has_shot := _try_shoot(delta)
		var direction := Vector3()
		var joystick_direction := _get_joystick_direction()

		direction.x += joystick_direction.y
		direction.z -= joystick_direction.x

		if has_shot:
			_velocity += direction * _MOVEMENT_SPEED
	else:
		var deceleration := _velocity.normalized() * (_DAMPING_FACTOR * _velocity.length_squared())
		_velocity -= deceleration * delta
		shoot_cooldown = 0
	_velocity.y = -_GRAVITY
	_velocity = move_and_slide(_velocity)

	if _floor:
		var height_from_floor := global_transform.origin.y - _floor.global_transform.origin.y
		if height_from_floor < -_FALL_THRESHOLD_DISTANCE:
			emit_signal("fell_off")

	damage_cooldown -= delta

func stop_moving() -> void:
	velocity *= 0.1

func on_damaged_by(entity: Enemy) -> void:
	if damage_cooldown <= 0:
		damage_cooldown = _DAMAGE_INTERVAL
		var old_health := health
		health -= entity.DAMAGE
		health = max(health, 0)
		emit_signal("health_changed", old_health, health)

func on_healed(heal: int) -> void:
	var old_health = health
	health += heal
	health = min(MAX_HEALTH, health)
	emit_signal("health_changed", old_health, health)
