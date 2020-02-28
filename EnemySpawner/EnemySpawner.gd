extends Area
class_name EnemySpawner

const Spider = preload("res://Enemy/Spider/Spider.tscn")
const Zombie = preload("res://Enemy/Zombie/Zombie.tscn")

onready var _level = get_tree().get_root()

const max_count := 15
const _respawn_time := 2

var _spawn_cooldown: float = 0
var _is_running := false
var _wave: Wave

signal spawned

func start(wave: Wave) -> void:
	_wave = wave
	connect("spawned", _wave, "on_enemy_spawned")
	_is_running = true
	
func stop(wave: Wave) -> void:
	_is_running = false
	
func _ready() -> void:
	randomize()
	add_to_group("spawners")

func _process(delta: float) -> void:
	if _is_running:
		if _spawn_cooldown <= 0 and _wave.spawned_count < _wave.total_count:
			var enemy: Enemy = _get_next_enemy_type().instance()
			enemy.transform.origin = global_transform.origin + Vector3(randf() * scale.x, -enemy.scale.y, randf() * scale.z)
			enemy.connect("died", _wave, "on_enemy_died")
			_level.add_child(enemy)
			emit_signal("spawned", self, enemy)
			_spawn_cooldown += _respawn_time
		_spawn_cooldown -= delta

func _get_next_enemy_type():
	if randf() < 0.5:
		return Zombie
	else:
		return Spider
