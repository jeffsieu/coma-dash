extends RichTextLabel

var current_wave: Wave

func _ready() -> void:
	text = "Zombies left"
	var level_manager := $"/root/Level/LevelManager"
	level_manager.connect("wave_changed", self, "on_wave_changed")
	
func _on_wave_changed(wave: Wave) -> void:
	current_wave = wave
	current_wave.connect("on_enemy_spawned", self, "on_spawned_zombie_count_changed")
	
func on_spawned_zombie_count_changed(spawned_zombie_count: int) -> void:
	text = str(spawned_zombie_count) + "/" + current_wave.zombie_count
