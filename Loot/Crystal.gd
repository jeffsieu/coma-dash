extends KinematicBody

signal collected

var is_flying = false
var velocity = Vector3()

var Player = load("res://Player/Player.gd")
var player

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
	
func _physics_process(delta):
	if is_flying:
		var distance_to_player = (player.transform.origin - transform.origin).length()
		velocity = move_and_slide((player.transform.origin - transform.origin).normalized() / pow(distance_to_player, 2) * 100)
		for i in range(get_slide_count()):
			var collision = get_slide_collision(i)
			if collision.collider is Player:
				emit_signal("collected")
				_die()

func collect(player):
	is_flying = true
	self.player = player

func _die() -> void:
	.queue_free()
