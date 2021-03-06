extends Node
class_name LootManager

const Collectible = preload("res://Collectible/Collectible.gd")

var _level_manager
var _player
var _drops := []
var _cleared = false

var crystal_count := 0
var exp_count := 0

signal player_healed
signal drops_collected

func _init(level_manager) -> void:
	_level_manager = level_manager
	_player = level_manager.find_node("Player")
	randomize()

func _on_crate_died(crate: Crate) -> void:
	drop_loots(crate)

func _on_enemy_died(enemy: Enemy) -> void:
	drop_loots(enemy)

func drop_loots(entity) -> void:
	for drop_item in entity.generate_drops():
		drop_loot(entity, drop_item)

func drop_loot(entity, item: Collectible) -> void:
	item.connect("collected", self, "_on_loot_collected")
	# offset the item position by a random amount so that the exp orb doesnt sit right under the crystal
	var offset := Vector3((randi() % 10 - 20) / 10.0, 0, (randi() % 20 - 10) / 10.0)
	item.translation = entity.global_transform.origin + offset
	_level_manager.add_child(item)
	_drops.append(item)

	# if this crate is destroyed after the level is cleared
	if _cleared:
		item.fly_to(_player)
	
func _on_loot_collected(item: Collectible) -> void:
	match item.TYPE:
		Collectible.CRYSTAL:
			crystal_count += item.value
		Collectible.EXPORB:
			exp_count += item.value
		Collectible.HEAL:
			emit_signal("player_healed", item.value)

	_drops.remove(_drops.find(item))
	_level_manager.on_loot_collected()

	if _cleared and _drops.empty():
		emit_signal("drops_collected")
		_cleared = false

func collect_all_items() -> void:
	_cleared = true
	for item in _drops:
		item.fly_to(_player)
