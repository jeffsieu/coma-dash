var total_count
var level_manager
var spawned_count = 0
var dead_count = 0

signal enemy_died
signal ended

func _init(level_manager, total_count):
	self.level_manager = level_manager
	self.total_count = total_count

func start():
	level_manager.get_tree().call_group("spawners", "start", self)
	
func stop_spawners():
	level_manager.get_tree().call_group("spawners", "stop")
	
func end():
	emit_signal("ended", self)
	
func on_enemy_spawned():
	spawned_count += 1
	if spawned_count >= total_count:
		stop_spawners()

func on_enemy_died():
	dead_count += 1
	emit_signal("enemy_died", dead_count, total_count)
	if dead_count == total_count:
		end()
