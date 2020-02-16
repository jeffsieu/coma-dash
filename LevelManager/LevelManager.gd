extends Node

const Wave = preload("res://LevelManager/Wave.gd")

var current_wave
var current_wave_count = 0
var wave_count = 5

signal wave_changed
signal enemy_spawned

func _ready():
	spawn_wave()
	
func end():
	pass

func spawn_wave():
	var wave = Wave.new(self, 5)
	wave.connect("spawned_count_changed", self, "on_spawned_count_changed")
	wave.connect("ended", self, "on_wave_ended")
	current_wave = wave
	current_wave_count += 1
	wave.start()
	emit_signal("wave_changed", current_wave, current_wave_count, wave_count)
	
func on_spawned_count_changed(spawned, total):
	emit_signal("enemy_spawned", spawned, total)
	
func on_wave_ended(wave):
	if current_wave_count == wave_count:
		end()
	else:
		spawn_wave()
