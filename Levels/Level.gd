extends Node
class_name Level

var current_wave: Wave
var current_wave_count: int = 1
var wave_count: int
var mobs_per_wave: int

var next_level: String

signal wave_changed

func _ready() -> void:
	current_wave_count = 0

func spawn_wave(level_manager) -> void:
	current_wave = Wave.new(self, mobs_per_wave)
	current_wave.connect("enemy_died", level_manager, "_on_enemy_died")
	current_wave.connect("ended", level_manager, "_on_wave_ended")
	current_wave_count += 1
	emit_signal("wave_changed", current_wave, current_wave_count, wave_count)
	current_wave.start()

func free_level() -> void:
	queue_free()
