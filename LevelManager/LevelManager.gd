extends Node

const Wave = preload("res://LevelManager/Wave.gd")

var current_wave
var current_wave_count = 0
var wave_count = 5

signal wave_changed
signal enemy_died

func _ready():
	spawn_wave()
	
func end():
	pass

func spawn_wave():
	current_wave = Wave.new(self, 5)
	current_wave.connect("enemy_died", self, "on_enemy_died")
	current_wave.connect("ended", self, "on_wave_ended")
	emit_signal("wave_changed", current_wave, current_wave_count, wave_count)
	current_wave_count += 1
	current_wave.start()
	
func on_enemy_died(died, total):
	emit_signal("enemy_died", died, total)
	
func on_wave_ended(wave):
	if current_wave_count == wave_count:
		end()
	else:
		spawn_wave()
