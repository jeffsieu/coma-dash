extends MarginContainer

onready var _scene_manager: SceneManager = get_tree().get_root().find_node("SceneManager")
onready var _enemies_defeated: Label = find_node("EnemiesDefeated")
onready var _crystal_count: Label = find_node("CrystalCount")

func _ready():
	pass

func set_score(enemies_died: int, crystal_count: int) -> void:
	_enemies_defeated.text = "Enemies defeated: %d" % enemies_died
	_crystal_count.text = "Crystals collected: %d" % crystal_count

func _on_RetryButton_pressed() -> void:
	get_tree().paused = false
	_scene_manager.load_scene("res://LevelManager/LevelManager.tscn")

func _on_HomeButton_pressed() -> void:
	get_tree().paused = false
	_scene_manager.load_scene("res://Scene/MainMenuScene.tscn")
