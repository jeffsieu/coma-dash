extends VBoxContainer

func _ready() -> void:
	$MarginContainer/VBoxContainer/PlayButton.connect("pressed", self, "_on_play_button_pressed")

func _on_play_button_pressed() -> void:
	var scene_manager: SceneManager = get_tree().get_root().get_node("SceneManager")
	scene_manager.load_scene("res://MainLevel.tscn")
