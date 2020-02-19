extends Node
class_name LootManager

const Collectible = preload("res://Loot/Collectible.gd")

var _level_manager

var crystal_count := 0
var exp_count := 0

func _init(level_manager) -> void:
	_level_manager = level_manager
	randomize()
	
func _on_enemy_died(enemy: Enemy) -> void:
	drop_loot_enemy(enemy)

func drop_loot_enemy(enemy: Enemy) -> void:
	for drop_item in enemy.generate_drops():
		drop_loot(enemy, drop_item)
	
func drop_loot(entity: Enemy, item: Collectible) -> void:
	item.connect("collected", self, "_on_loot_collected")
	# offset the item position by a random amount so that the exp orb doesnt sit right under the crystal
	var offset := Vector3((randi() % 10 - 20) / 10.0, 0, (randi() % 20 - 10) / 10.0)
	item.transform.origin = entity.transform.origin + offset
	_level_manager.add_child(item)
	
func _on_loot_collected(item: Collectible) -> void:
	match item.type:
		Collectible.CRYSTAL:
			crystal_count += item.value
		Collectible.EXPORB:
			exp_count += item.value
	_level_manager.on_loot_collected()
