extends Node
class_name LootManager

const Crystal = preload("res://Loot/Crystal.tscn")

var level_manager
var chance := 1.0/3.0

func _init(level_manager) -> void:
	self.level_manager = level_manager
	
func _on_enemy_died(enemy: Enemy, died: int, total: int) -> void:
	maybe_drop_loot_enemy(enemy)

func maybe_drop_loot_enemy(enemy: Enemy) -> void:
	if randf() < chance:
		drop_loot(enemy)
	
func drop_loot(entity: Enemy) -> void:
	var crystal = Crystal.instance()
	crystal.connect("collected", self, "_on_loot_collected")
	crystal.transform.origin = entity.transform.origin
	level_manager.add_child(crystal)
	
func _on_loot_collected(item) -> void:
	level_manager.on_loot_collected(item)
