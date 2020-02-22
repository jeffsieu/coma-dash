extends Control
class_name GUI

var _player: Player

onready var level := get_tree().get_root().find_node("Level", true, false)
onready var _wave_counter := level.find_node("WaveCounter")
onready var _level_manager := level.get_node("LevelManager")
onready var _zombies_left := level.find_node("ZombiesLeft")
onready var _player_health_bar := level.find_node("PlayerHealthBar")
onready var _player_health_points := level.find_node("HealthPoints")
onready var _crystal_count := level.find_node("CrystalCount")
onready var _player_exp_bar := level.find_node("PlayerExpBar")
onready var _pause_button := level.find_node("PauseButton")
onready var _resume_button := level.find_node("ResumeButton")
onready var _quit_button := level.find_node("QuitButton")
onready var _pause_menu := level.find_node("PauseMenu")

func _ready() -> void:
	_player =  level.get_node("Player")

	_level_manager.connect("wave_changed", self, "_on_wave_changed")
	_level_manager.connect("enemy_died", self, "_on_enemy_died")
	_level_manager.connect("loot_collected", self, "_on_loot_collected")
	_player.connect("health_changed", self, "_on_player_health_changed")
	_pause_button.connect("pressed", self, "_on_pause_button_pressed")
	_resume_button.connect("pressed", self, "_on_resume_button_pressed")
	_quit_button.connect("pressed", self, "_on_quit_button_pressed")

	_wave_counter.text = "WAVE %s/%s" % [_level_manager.current_wave_count, _level_manager.wave_count]
	_zombies_left.text = "%d ZOMBIES LEFT" % _level_manager.current_wave.total_count
	_player_health_bar.max_value = _player.max_health
	_player_health_bar.value = _player.health
	_player_health_points.text = "%d/%d" % [_player.health, _player.max_health]
	_pause_menu.set_visible(false)
	
func _on_wave_changed(wave: Wave, wave_count: int, total_wave_count: int) -> void:
	_wave_counter.text = "WAVE %d/%d" % [wave_count, total_wave_count]
	_zombies_left.text = "%d ZOMBIES LEFT" % wave.total_count
	
func _on_enemy_died(enemy: Enemy, died: int, total: int) -> void:
	_zombies_left.text = "%d ZOMBIES LEFT" % (total - died)
	
func _on_player_health_changed(old: int, new: int) -> void:
	_player_health_points.text = "%d/%d" % [new, _player.max_health]
	var tween: Tween = _player_health_bar.get_node("Tween")
	tween.interpolate_method(_player_health_bar, "set_value", old, new, 0.25, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
	tween.start()
	
func _on_loot_collected() -> void:
	_crystal_count.text = "x%d" % (_level_manager.loot_manager.crystal_count)
	_player_exp_bar.value = _level_manager.loot_manager.exp_count

func _on_pause_button_pressed() -> void:
	get_tree().paused = true
	_pause_menu.set_visible(true)

func _on_resume_button_pressed() -> void:
	_pause_menu.set_visible(false)
	get_tree().paused = false

func _on_quit_button_pressed() -> void:
	get_tree().paused = false
	var scene_manager: SceneManager = get_tree().get_root().get_node("SceneManager")
	scene_manager.load_scene("res://Scene/MainMenuScene.tscn")
