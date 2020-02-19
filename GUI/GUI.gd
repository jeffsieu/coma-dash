extends Control
class_name GUI

var _level_manager: LevelManager
var _player: Player

func _ready() -> void:
	var level := get_tree().get_root().find_node("Level", true, false)
	_level_manager = level.get_node("LevelManager")
	_player =  level.get_node("Player")

	_level_manager.connect("wave_changed", self, "_on_wave_changed")
	_level_manager.connect("enemy_died", self, "_on_enemy_died")
	_level_manager.connect("loot_collected", self, "_on_loot_collected")
	_player.connect("health_changed", self, "_on_player_health_changed")	

	$WaveCounter.text = "WAVE %s/%s" % [_level_manager.current_wave_count, _level_manager.wave_count]
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % _level_manager.current_wave.total_count
	$PlayerHealthBarContainer/PlayerHealthBar.max_value = _player.max_health
	$PlayerHealthBarContainer/PlayerHealthBar.value = _player.health
	$PlayerHealthBarContainer/CenterContainer/HealthPoints.text = "%d/%d" % [_player.health, _player.max_health]
	
func _on_wave_changed(wave: Wave, wave_count: int, total_wave_count: int):
	$WaveCounter.text = "WAVE %d/%d" % [wave_count, total_wave_count]
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % wave.total_count
	
func _on_enemy_died(enemy: Enemy, died: int, total: int):
	$ZombiesLeft.text = "%d ZOMBIES LEFT" % (total - died)
	
func _on_player_health_changed(old: int, new: int):
	$PlayerHealthBarContainer/PlayerHealthBar.value = new
	$PlayerHealthBarContainer/CenterContainer/HealthPoints.text = "%d/%d" % [new, _player.max_health]
	
func _on_loot_collected():
	$LootDisplay/LootContainer/CrystalCount.text = "x%d" % (_level_manager.loot_manager.crystal_count)
	$LootDisplay/LootContainer/PlayerExpBar.value = _level_manager.loot_manager.exp_count
