extends VBoxContainer

var level_manager
var player

func _ready():
	level_manager = $"/root/Level/LevelManager"
	player = $"/root/Level/Player"
	
	level_manager.connect("wave_changed", self, "_on_wave_changed")
	level_manager.connect("enemy_died", self, "_on_enemy_died")
	player.connect("health_changed", self, "_on_player_health_changed")	

	$WaveCounter.text = "WAVE %s/%s" % [level_manager.current_wave_count, level_manager.wave_count]
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % level_manager.current_wave.total_count
	$PlayerHealthBarContainer/PlayerHealthBar.max_value = player.max_health
	$PlayerHealthBarContainer/PlayerHealthBar.value = player.health
	$PlayerHealthBarContainer/CenterContainer/HealthPoints.text = "%d/%d" % [player.health, player.max_health]
	
func _on_wave_changed(wave, wave_count, total_wave_count):
	$WaveCounter.text = "WAVE %d/%d" % [wave_count, total_wave_count]
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % wave.total_count
	
func _on_enemy_died(died, total):
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % (total - died)
	
func _on_player_health_changed(old, new):
	$PlayerHealthBarContainer/PlayerHealthBar.value = new
	$PlayerHealthBarContainer/CenterContainer/HealthPoints.text = "%d/%d" % [new, player.max_health]

