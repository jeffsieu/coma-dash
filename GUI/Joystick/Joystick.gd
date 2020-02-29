extends Sprite

onready var _level_manager = get_tree().get_root().find_node("LevelManager", true, false)
onready var _player = _level_manager.find_node("Player")
onready var _knob = find_node("JoystickKnob")

var enabled := true

func _ready() -> void:
	visible = false

func enable() -> void:
	enabled = true

func disable() -> void:
	_player.is_shooting = false
	_knob.reset_position()
	visible = false
	enabled = false

func _input(event: InputEvent) -> void:
	if enabled:
		if event is InputEventScreenTouch and event.is_pressed():
			visible = true
			set_global_position(event.position)
			_knob.reset_position()
			_player.is_shooting = true
		elif event is InputEventScreenTouch:
			visible = false
			_player.is_shooting = false
			_knob.reset_position()
