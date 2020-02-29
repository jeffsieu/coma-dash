extends MarginContainer

onready var _enemies_defeated: Label = get_node("VBoxContainer/VBoxContainer/EnemiesDefeated")
onready var _crystal_count: Label = get_node("VBoxContainer/VBoxContainer/CrystalCount")

signal proceed_next

func set_score(enemies_died: int, crystal_count: int) -> void:
	_enemies_defeated.text = "Enemies defeated: %d" % enemies_died
	_crystal_count.text = "Crystals collected: %d" % crystal_count

func _on_TapNext_pressed() -> void:
	emit_signal("proceed_next")
