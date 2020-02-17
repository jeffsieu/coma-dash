extends Area
class_name EnemySpawner

const Zombie = preload("res://Enemy/Zombie.tscn")
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
	add_to_group("spawners")

func _process(delta: float) -> void:
	if _is_running:
		if _spawn_cooldown <= 0 and _wave.spawned_count < _wave.total_count:
			var enemy: Enemy = _get_next_enemy_type().instance()
			enemy.transform.origin = transform.origin + Vector3(randf() * scale.x, -enemy.scale.y, randf() * scale.z)
			enemy.connect("died", _wave, "on_enemy_died")
			$"/root/Level".add_child(enemy)
			emit_signal("spawned", self, enemy)
			_spawn_cooldown += _respawn_time
		_spawn_cooldown -= delta

func _get_next_enemy_type():
	return Zombie
