extends Crate

const HealthBoost = preload("res://Collectible/HealthBoost/HealthBoost.tscn")

const MAX_HEALTH = 20

func _ready() -> void:
	health = MAX_HEALTH
	emit_signal("health_changed", self, health)

func generate_drops() -> Array:
	var drops := Array()
	var healthBoost := HealthBoost.instance()
	healthBoost.value = 20
	drops.append(healthBoost)
	return drops
