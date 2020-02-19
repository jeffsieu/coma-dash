extends Spatial
class_name SceneManager

onready var game_layer = $GameLayer

func load_scene(scene_path):
	if game_layer.get_child_count() == 1:
		var child = game_layer.get_children()[0]
		game_layer.remove_child(child)
		child.call_deferred("free")
	game_layer.add_child(load(scene_path).instance())
