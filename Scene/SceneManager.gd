extends Spatial
class_name SceneManager

onready var _game_layer = $GameLayer

func load_scene(scene_path):
	if _game_layer.get_child_count() == 1:
		var child = _game_layer.get_children()[0]
		_game_layer.remove_child(child)
		child.call_deferred("free")
	_game_layer.add_child(load(scene_path).instance())
