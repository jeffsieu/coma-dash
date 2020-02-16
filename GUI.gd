extends VBoxContainer

var level_manager

func _ready():
	var level_manager = $"/root/Level/LevelManager"
	level_manager.connect("wave_changed", self, "on_wave_changed")
	level_manager.connect("enemy_spawned", self, "on_enemy_spawned")
	$WaveCounter.text = "WAVE %s/%s" % [level_manager.current_wave_count, level_manager.wave_count]
	
func on_wave_changed(wave, wave_count, total_wave_count):
	$WaveCounter.text = "WAVE %d/%d" % [wave_count, total_wave_count]
	
func on_enemy_spawned(spawned, total):
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % (total - spawned)
	

	

