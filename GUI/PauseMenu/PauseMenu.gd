extends MarginContainer

func _on_resume_button_pressed() -> void:
	set_visible(false)
	get_tree().paused = false
	
func _on_quit_button_pressed() -> void:
	get_tree().paused = false
	var scene_manager: SceneManager = get_tree().get_root().find_node("SceneManager")
	scene_manager.load_scene("res://Scene/MainMenuScene.tscn")
