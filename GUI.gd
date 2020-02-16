extends VBoxContainer

var level_manager

func _ready():
	level_manager = $"../LevelManager"
	level_manager.connect("wave_changed", self, "on_wave_changed")
	level_manager.connect("enemy_died", self, "on_enemy_died")
	level_manager.connect("loot_collected", self, "on_loot_collected")
	$WaveCounter.text = "WAVE %s/%s" % [level_manager.current_wave_count, level_manager.wave_count]
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % level_manager.current_wave.total_count
	
func on_wave_changed(wave, wave_count, total_wave_count):
	$WaveCounter.text = "WAVE %d/%d" % [wave_count, total_wave_count]
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % wave.total_count

func on_enemy_died(died, total):
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % (total - died)
	
func on_loot_collected(collected):
	$LootCollected.text = "%d CRYSTALS COLLECTED" % (collected)
