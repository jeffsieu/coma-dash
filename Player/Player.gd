extends KinematicBody
class_name Player

const Bullet = preload("res://Bullet/Bullet.tscn")

onready var _level = get_tree().get_root().find_node("Level", true, false)

const movement_speed := 2.5
const velocity_limit := 5
const acceleration := 0.2
const gravity := 5
const friction := 0.6
const max_health := 200

const shoot_interval := 0.1
var shoot_cooldown: float = 0

const damage_interval = 0.5
var damage_cooldown: float = damage_interval

var health: int = max_health
var velocity: Vector3 = Vector3()
var is_shooting := false

signal health_changed

func _ready() -> void:
	$ItemCollector.connect("area_entered", self, "_on_item_near")

func _get_joystick_direction() -> Vector3:
	return _level.get_node("Joystick/JoystickKnob").joystick_direction
	
func _try_shoot(delta: float) -> bool:
	var has_shot := shoot_cooldown <= 0
	if has_shot:
		shoot_cooldown += shoot_interval
		var bullet := Bullet.instance()
		bullet.transform.origin = transform.origin + transform.basis.z * 2.2
		bullet.rotation = rotation
		_level.add_child(bullet)
	shoot_cooldown -= delta
	return has_shot
			
func _rotate_body() -> void:
	var direction = _get_joystick_direction()
	if direction != Vector2():
		rotation.y = -direction.angle()
		
func _on_item_near(item: Area) -> void:
	if item and item.get_parent() is Collectible:
		item.get_parent().fly_to(self)

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
			velocity += direction * movement_speed
	else:
		var deceleration := velocity.normalized() * (friction * velocity.length_squared())
		# velocity = velocity * pow((1 - friction), delta)
		velocity -= deceleration * delta
		shoot_cooldown = 0
	velocity.y = -gravity
	velocity = move_and_slide(velocity)
	
	damage_cooldown -= delta

func on_damaged_by(entity: Enemy) -> void:
	if damage_cooldown <= 0:
		damage_cooldown = damage_interval
		var old_health := health
		health -= entity.damage
		emit_signal("health_changed", old_health, health)

func on_healed(heal: int) -> void:
	var old_health = health
	health += heal
	health = min(max_health, health)
	emit_signal("health_changed", old_health, health)
