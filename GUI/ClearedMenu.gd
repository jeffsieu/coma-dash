extends MarginContainer

onready var _enemies_defeated: Label = find_node("EnemiesDefeated")
onready var _crystal_count: Label = find_node("CrystalCount")

signal proceed_next

func set_score(enemies_died: int, crystal_count: int) -> void:
	_enemies_defeated.text = "Enemies defeated: %d" % enemies_died
	_crystal_count.text = "Crystals collected: %d" % crystal_count

func _on_TapNext_pressed() -> void:
	emit_signal("proceed_next")
