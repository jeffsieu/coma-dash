extends Control
class_name GUI


onready var _level_manager := get_tree().get_root().find_node("LevelManager", true, false)
onready var _player: Player = _level_manager.find_node("Player")

onready var _wave_counter := _level_manager.find_node("WaveCounter")
onready var _enemies_left := _level_manager.find_node("EnemiesLeft")
onready var _player_health_bar := _level_manager.find_node("PlayerHealthBar")
onready var _player_health_points := _level_manager.find_node("HealthPoints")
onready var _crystal_count := _level_manager.find_node("CrystalCount")
onready var _player_exp_bar := _level_manager.find_node("PlayerExpBar")
onready var _pause_menu := _level_manager.find_node("PauseMenu")
onready var _cleared_menu := _level_manager.find_node("ClearedMenu")
onready var _game_over := _level_manager.find_node("GameOver")

var _level: Level

signal to_next_level_pressed

func _ready() -> void:
	_level_manager.connect("enemy_died", self, "_on_enemy_died")
	_level_manager.connect("loot_collected", self, "_on_loot_collected")
	_level_manager.connect("level_cleared", self, "_on_level_cleared")
	_level_manager.connect("game_over", self, "_on_game_over")
	_level_manager.connect("level_loaded", self, "_on_level_loaded")
	_player.connect("health_changed", self, "_on_player_health_changed")
	_cleared_menu.connect("to_next_level_pressed", self, "_on_to_next_level_pressed")
	self.connect("to_next_level_pressed", _level_manager, "on_to_next_level_pressed")

	_player_health_bar.max_value = _player.MAX_HEALTH
	_player_health_bar.value = _player.health
	_player_health_points.text = "%d/%d" % [_player.health, _player.MAX_HEALTH]
	_pause_menu.set_visible(false)
	_cleared_menu.set_visible(false)

func _on_level_loaded(level: Level) -> void:
	_level = level
	_level.connect("wave_changed", self, "_on_wave_changed")
	_wave_counter.text = "WAVE %s/%s" % [_level.current_wave_count, _level.total_wave_count]
	_enemies_left.text = "%d ENEMIES LEFT" % _level.mobs_per_wave

func _on_wave_changed(wave: Wave, wave_count: int, total_wave_count: int) -> void:
	_wave_counter.text = "WAVE %d/%d" % [wave_count, total_wave_count]
	_enemies_left.text = "%d ENEMIES LEFT" % wave.total_count

func _on_enemy_died(enemy: Enemy) -> void:
	var wave: Wave = _level.current_wave
	_enemies_left.text = "%d ENEMIES LEFT" % (wave.total_count - wave.dead_count)

func _on_player_health_changed(old: int, new: int) -> void:
	_player_health_points.text = "%d/%d" % [new, _player.MAX_HEALTH]
	var tween: Tween = _player_health_bar.find_node("Tween")
	tween.interpolate_method(_player_health_bar, "set_value", old, new, 0.25, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
	tween.start()

func _on_loot_collected() -> void:
	_crystal_count.text = "x%d" % (_level_manager.loot_manager.crystal_count)
	_player_exp_bar.value = _level_manager.loot_manager.exp_count

func _on_pause_button_pressed() -> void:
	get_tree().paused = true
	_pause_menu.set_visible(true)

func _on_game_over(enemies_died, crystal_count) -> void:
	get_tree().paused = true
	_game_over.set_score(enemies_died, crystal_count)
	_game_over.set_visible(true)

func _on_level_cleared(enemies_died, crystal_count) -> void:
	get_tree().paused = true
	_cleared_menu.set_score(enemies_died, crystal_count)
	_cleared_menu.set_visible(true)

func _on_to_next_level_pressed() -> void:
	get_tree().paused = false
	_cleared_menu.set_visible(false)
	emit_signal("to_next_level_pressed")
