extends VBoxContainer

onready var _scene_manager: SceneManager = get_tree().get_root().find_node("SceneManager", true, false)

func _ready() -> void:
	$MarginContainer/VBoxContainer/PlayButton.connect("pressed", self, "_on_play_button_pressed")

func _on_play_button_pressed() -> void:
	_scene_manager.load_scene("res://LevelManager/LevelManager.tscn")
