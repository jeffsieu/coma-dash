extends RichTextLabel

var current_wave

func _ready():
	text = "Zombies left"
	var level_manager = $"/root/Level/LevelManager"
	level_manager.connect("wave_changed", self, "on_wave_changed")
	
func _on_wave_changed(wave):
	current_wave = wave
	current_wave.connect("game_over", self, "on_spawned_zombie_count_changed")
	
func on_spawned_zombie_count_changed(spawned_zombie_count):
	text = str(spawned_zombie_count) + "/" + current_wave.zombie_count
