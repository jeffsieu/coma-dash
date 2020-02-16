extends Node

const Wave = preload("res://LevelManager/Wave.gd")
const LootManager = preload("res://LevelManager/LootManager.gd")

var current_wave
var current_wave_count = 0
var wave_count = 5
var loot_count = 0
var loot_manager

signal wave_changed
signal enemy_died
signal loot_collected

onready var player = $"/root/Level/Player"

func _ready():
	loot_manager = LootManager.new(self)
	_spawn_wave()
	player.connect("health_changed", self, "_on_player_health_changed")
	
func _end():
	print("Game over")
	current_wave_count = 0
	_spawn_wave()

func _spawn_wave():
	current_wave = Wave.new(self, 5, loot_manager)
	current_wave.connect("enemy_died", self, "_on_enemy_died")
	current_wave.connect("ended", self, "_on_wave_ended")
	emit_signal("wave_changed", current_wave, current_wave_count, wave_count)
	current_wave_count += 1
	current_wave.start()
	
func _on_enemy_died(died, total):
	emit_signal("enemy_died", died, total)
	
func _on_wave_ended(wave):
	if current_wave_count == wave_count:
		_end()
	else:
		_spawn_wave()

func _on_player_health_changed(old, new):
	if new <= 0:
		_end()
	
func on_loot_collected():
	loot_count += 1
	emit_signal("loot_collected", loot_count)
