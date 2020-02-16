extends Node

var zombie_count
var spawned_zombie_count = 0

signal spawned_zombie_count_changed

func _init(zombie_count):
	self.zombie_count = zombie_count
	emit_signal("spawned_zombie_count_changed")

func start():
	get_tree().call_group("spawners", "start", self)
	
func zombie_spawned():
	spawned_zombie_count += 1
	emit_signal("spawned_zombie_count_changed", spawned_zombie_count)
