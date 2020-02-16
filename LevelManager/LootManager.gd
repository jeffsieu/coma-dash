extends Node

const Crystal = preload("res://Loot/Crystal.tscn")

var level_manager

var chance = 1.0/3.0

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

func _init(level_manager):
	self.level_manager = level_manager

func maybe_drop_loot_enemy(enemy) -> void:
	if randf() < chance:
		drop_loot(enemy)
	
func drop_loot(node) -> void:
	var crystal = Crystal.instance()
	crystal.connect("collected", self, "on_loot_collected")
	crystal.transform.origin = node.transform.origin
	level_manager.add_child(crystal)
	
func on_loot_collected():
	level_manager.on_loot_collected()
