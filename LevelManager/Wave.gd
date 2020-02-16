var zombie_count
var level_manager
var spawned_zombie_count = 0

signal spawned_zombie_count_changed

func _init(level_manager, zombie_count):
	self.level_manager = level_manager
	self.zombie_count = zombie_count
	emit_signal("spawned_zombie_count_changed")

func start():
	level_manager.get_tree().call_group("spawners", "start", self)
	
func zombie_spawned():
	spawned_zombie_count += 1
	emit_signal("spawned_zombie_count_changed", spawned_zombie_count)
