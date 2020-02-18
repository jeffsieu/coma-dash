extends MarginContainer

onready var _scene_manager: SceneManager = get_tree().get_root().get_node("SceneManager")

func _ready():
	
	$VBoxContainer/VBoxContainer2/HBoxContainer/HomeButton.connect("pressed", self, "_on_home_button_pressed")
	$VBoxContainer/VBoxContainer2/RetryButton.connect("pressed", self, "_on_retry_button_pressed")

func _on_retry_button_pressed() -> void:
	_scene_manager.load_scene("res://Scene/MainLevel.tscn")

func _on_home_button_pressed() -> void:
	var scene_manager: SceneManager = get_tree().get_root().get_node("SceneManager")
	_scene_manager.load_scene("res://Scene/MainMenuScene.tscn")
