extends Spatial

var count = 0
var max_count = 15
var respawn_time = 5
var respawn_cooldown = 0
var zombie_resource = preload("res://Zombie/Zombie.tscn")

func _process(delta):
	
	if respawn_cooldown <= 0:
		respawn_cooldown += respawn_time
		count += 1
		var zombie = zombie_resource.instance()
		var floor_node = $"Floor"
		randomize() 
		zombie.transform.origin = Vector3(rand_range(-floor_node.scale.x, floor_node.scale.x), -zombie.scale.y, rand_range(-floor_node.scale.z, floor_node.scale.z))
		$"..".add_child(zombie)
	respawn_cooldown -= delta
