extends Node

const Wave = preload("res://LevelManager/Wave.gd")

var current_wave

signal wave_changed

func _ready():
	spawn_wave()
	pass # Replace with function body.

func spawn_wave():
	var wave = Wave.new(5)
	current_wave = wave
	emit_signal("wave_changed", current_wave)
	wave.start()

#func _process(delta):
#	pass
