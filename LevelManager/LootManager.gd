extends Node

const Crystal = preload("res://Loot/Crystal.tscn")

var level_manager

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

func _init(level_manager):
	self.level_manager = level_manager

func maybe_attach_loot_enemy(enemy) -> void:
	attach_loot_enemy(enemy)
	
func attach_loot_enemy(enemy) -> void:
	enemy.has_loot = true
	
func drop_loot(enemy) -> void:
	var crystal = Crystal.instance()
	crystal.connect("collected", self, "on_loot_collected")
	crystal.transform.origin = enemy.transform.origin
	level_manager.add_child(crystal)
	
func on_loot_collected():
	level_manager.on_loot_collected()
