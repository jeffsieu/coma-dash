extends Area

const Zombie = preload("res://Zombie/Zombie.tscn")
const max_count = 15
const respawn_time = 2

var count = 0
var respawn_cooldown = 0
var wave

func _ready():
	add_to_group("spawners")

func start(wave):
	self.wave = wave

func _process(delta):
	if respawn_cooldown <= 0:
		respawn_cooldown += respawn_time
		count += 1
		var zombie = Zombie.instance(wave)
		randomize() 
		zombie.transform.origin = transform.origin + Vector3(randf() * scale.x, -zombie.scale.y, randf() * scale.z)
		$"..".add_child(zombie)
		wave.spawned_zombie_count += 1
	respawn_cooldown -= delta
