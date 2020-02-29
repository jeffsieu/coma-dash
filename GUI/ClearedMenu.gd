extends MarginContainer

onready var _enemies_defeated: Label = find_node("EnemiesDefeated")
onready var _crystal_count: Label = find_node("CrystalCount")

var _pressed := false

signal proceed_next

func set_score(enemies_died: int, crystal_count: int) -> void:
	_enemies_defeated.text = "Enemies defeated: %d" % enemies_died
	_crystal_count.text = "Crystals collected: %d" % crystal_count

func _input(event: InputEvent) -> void:
	if is_visible_in_tree():
		if event is InputEventScreenTouch:
			if event.is_pressed():
				_pressed = true
			if _pressed and not event.is_pressed():
				emit_signal("proceed_next")
				_pressed = false
