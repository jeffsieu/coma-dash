extends Area

const Zombie = preload("res://Zombie/Zombie.tscn")
const max_count = 15
const respawn_time = 2

var count = 0
var respawn_cooldown = 0
var is_running = false
var wave

func _ready():
	add_to_group("spawners")

func start(wave):
	self.wave = wave
	is_running = true
	
func stop():
	is_running = false

func _process(delta):
	if is_running:
		if respawn_cooldown <= 0 and wave.spawned_count < wave.total_count:
			respawn_cooldown += respawn_time
			count += 1
			var zombie = Zombie.instance()
			zombie.wave = wave
			zombie.transform.origin = transform.origin + Vector3(randf() * scale.x, -zombie.scale.y, randf() * scale.z)
			$"/root/Level".add_child(zombie)
			wave.on_enemy_spawned(zombie)
		respawn_cooldown -= delta
