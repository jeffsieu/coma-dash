extends Crate

const HealthBoost = preload("res://Loot/HealthBoost.tscn")

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	max_health = 20
	health = max_health
	emit_signal("health_changed", self, health)

func generate_drops() -> Array:
	var drops := Array()
	var healthBoost := HealthBoost.instance()
	healthBoost.set_value(20)
	drops.append(healthBoost)
	return drops
