class_name Wave

var total_count: int
var level_manager
var spawned_count = 0
var dead_count = 0

signal enemy_died
signal ended

func _init(level_manager, total_count: int) -> void:
	self.level_manager = level_manager
	self.total_count = total_count

func start() -> void:
	level_manager.get_tree().call_group("spawners", "start", self)

func stop_spawners() -> void:
	level_manager.get_tree().call_group("spawners", "stop", self)

func end() -> void:
	emit_signal("ended", self)

func on_enemy_spawned(spawner, enemy: Enemy) -> void:
	spawned_count += 1
	if spawned_count >= total_count:
		stop_spawners()

func on_enemy_died(enemy: Enemy) -> void:
	dead_count += 1
	emit_signal("enemy_died", enemy)
	if dead_count == total_count:
		end()
