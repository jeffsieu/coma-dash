extends Node
class_name LevelManager

var _level

var current_wave: Wave
var current_wave_count: int = 0
var wave_count: int = 5
var loot_count: int = 0
var loot_manager: LootManager

signal wave_changed
signal enemy_died
signal loot_collected

func _ready() -> void:
	_level = get_tree().get_root().find_node("Level", true, false)
	loot_manager = LootManager.new(self)
	_spawn_wave()
	var player: Player = _level.get_node("Player")
	player.connect("health_changed", self, "_on_player_health_changed")
	self.connect("enemy_died", loot_manager, "_on_enemy_died")
	
func _end() -> void:
	var scene_manager: SceneManager = get_tree().get_root().get_node("SceneManager")
	scene_manager.load_scene("res://Scene/GameOverScene.tscn")

func _spawn_wave() -> void:
	current_wave = Wave.new(self, 5)
	current_wave.connect("enemy_died", self, "_on_enemy_died")
	current_wave.connect("ended", self, "_on_wave_ended")
	current_wave_count += 1
	emit_signal("wave_changed", current_wave, current_wave_count, wave_count)
	current_wave.start()
	
func _on_enemy_died(enemy: Enemy) -> void:
	emit_signal("enemy_died", enemy)
	
func _on_wave_ended(wave: Wave) -> void:
	if current_wave_count == wave_count:
		_end()
	else:
		_spawn_wave()

func _on_player_health_changed(old: int, new: int) -> void:
	if new <= 0:
		_end()
	
func on_loot_collected() -> void:
	emit_signal("loot_collected")
