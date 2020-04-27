extends Node

class_name Weapon

var _player

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.
	
func set_player(player) -> void:
	_player = player

func _try_shoot(delta: float) -> bool:
	return false
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
