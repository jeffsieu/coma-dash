extends Sprite3D

var max_health
var health

func _ready():
	texture = $Viewport.get_texture()

func set_max_health(max_health):
	self.max_health = max_health
	$Viewport/ProgressBar.set_max(self.max_health)

func update_health(health):
	self.health = health
	$Viewport/ProgressBar.set_value(self.health)
