extends Spatial

const Zombie = preload("res://Zombie/Zombie.tscn")
const max_count = 15
const respawn_time = 5

var count = 0
var respawn_cooldown = 0

func _process(delta):
	
	if respawn_cooldown <= 0:
		respawn_cooldown += respawn_time
		count += 1
		var zombie = Zombie.instance()
		var floor_node = $"Floor"
		randomize() 
		zombie.transform.origin = Vector3(rand_range(-floor_node.scale.x, floor_node.scale.x), -zombie.scale.y, rand_range(-floor_node.scale.z, floor_node.scale.z))
		$"..".add_child(zombie)
	respawn_cooldown -= delta
