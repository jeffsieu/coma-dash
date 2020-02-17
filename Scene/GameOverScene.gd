extends MarginContainer

func _ready():
	$VBoxContainer/VBoxContainer2/HBoxContainer/HomeButton.connect("pressed", self, "_on_home_button_pressed")
	$VBoxContainer/VBoxContainer2/RetryButton.connect("pressed", self, "_on_retry_button_pressed")

func _on_retry_button_pressed() -> void:
	get_tree().change_scene("res://Scene/MainLevel.tscn")

func _on_home_button_pressed() -> void:
	get_tree().change_scene("res://Scene/MainMenuScene.tscn")
