extends VBoxContainer

func _ready() -> void:
	$MarginContainer/VBoxContainer/PlayButton.connect("pressed", self, "_on_play_button_pressed")

func _on_play_button_pressed() -> void:
	get_tree().change_scene("res://MainLevel.tscn")
