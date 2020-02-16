extends Area

signal collected

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func _on_Crystal_body_entered(body: Node) -> void:
	if body.get_name() == "Player":
		emit_signal("collected")
		_die()

func _die() -> void:
	.queue_free()
