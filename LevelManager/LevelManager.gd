extends Node
class_name LevelManager

onready var _level_manager = get_tree().get_root().find_node("LevelManager", true, false)
onready var _level_position = _level_manager.find_node("LevelPosition")
onready var _player: Player = _level_manager.find_node("Player")
onready var _camera = _level_manager.find_node("Camera")
onready var _joystick = _level_manager.find_node("Joystick")

var _first_level := "res://Levels/Level1/Level1.tscn"
var _level_floor_distance := Vector3(0, 50, 0)
var level: Level
var _prev_level: Level

var _enemies_died := 0
var loot_count := 0
var loot_manager: LootManager

signal enemy_died
signal crate_died
signal game_over
signal loot_collected
signal level_cleared
signal level_loaded

func _ready() -> void:
	loot_manager = LootManager.new(self)

	_player.connect("health_changed", self, "_on_player_health_changed")
	_player.connect("fell_off", self, "_on_player_fell_off")
	loot_manager.connect("player_healed", _player, "on_healed")
	loot_manager.connect("drops_collected", self, "_on_drops_collected")
	self.connect("enemy_died", loot_manager, "_on_enemy_died")
	self.connect("crate_died", loot_manager, "_on_crate_died")

	_start()

func _cleared() -> void:
	level.get_tree().call_group("crates", "die")
	loot_manager.collect_all_items()
	_joystick.disable()

func _on_drops_collected() -> void:
	emit_signal("level_cleared", _enemies_died, loot_manager.crystal_count)

func _game_over() -> void:
	emit_signal("game_over", _enemies_died, loot_manager.crystal_count)

func _start() -> void:
	_load_level(_first_level)

func _on_crate_died(crate: Crate) -> void:
	emit_signal("crate_died", crate)

func _on_enemy_died(enemy: Enemy) -> void:
	_enemies_died += 1
	emit_signal("enemy_died", enemy)

func _on_wave_ended(wave: Wave) -> void:
	if level.current_wave_count == level.wave_count:
		_cleared()
	else:
		level.spawn_wave(self)

func _on_player_health_changed(old: int, new: int) -> void:
	if new <= 0:
		_game_over()
		
func _on_player_fell_off() -> void:
	_game_over()

func on_to_next_level_pressed() -> void:
	_prev_level = level
	
	if level.next_level:
		_load_level(level.next_level)

func _animate_to_next_level() -> void:
	var camera_origin: Vector3 = _camera.transform.origin
	var player_origin: Vector3 = _player.transform.origin
	var new_player_origin: Vector3 = level.find_node("PlayerPosition").global_transform.origin
	var duration := 1.2
	
	var tween: Tween = _level_manager.find_node("Tween")
	
	if _prev_level == null:
		_camera.translation = camera_origin + new_player_origin - player_origin
		_player.translation = new_player_origin
		_on_animated_to_next_level()
	else:
		tween.interpolate_property(_camera, "translation", camera_origin, camera_origin + new_player_origin - player_origin, duration, Tween.TRANS_SINE, Tween.EASE_IN_OUT)
		tween.interpolate_property(_player, "translation", player_origin, new_player_origin, duration, Tween.TRANS_SINE, Tween.EASE_IN_OUT)
		tween.interpolate_callback(self, duration * 1.5, "_on_animated_to_next_level")
		tween.start()

func _load_level(level_path: String) -> void:
	var LevelScene = load(level_path)
	level = LevelScene.instance()
	_level_manager.add_child(level)
	_level_position.transform.origin += _level_floor_distance
	level.transform.origin = _level_position.transform.origin
	
	_animate_to_next_level()

func _on_animated_to_next_level() -> void:
	if _prev_level:
		_level_manager.remove_child(_prev_level)
		_prev_level.queue_free()

	level.set_name("Level")
	level.spawn_wave(self)
	emit_signal("level_loaded", level)

	_joystick.enable()

func on_loot_collected() -> void:
	emit_signal("loot_collected")

func on_bullet_damaged(effectiveness: float) -> void:
	_camera.apply_damage_trauma(effectiveness)
