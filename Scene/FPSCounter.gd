extends Label

const _TIMER_LIMIT := 1.0
var _timer := 0.0

func _process(delta: float) -> void:
	_timer += delta
	if _timer > _TIMER_LIMIT: # Prints every 2 seconds
		_timer = 0.0
		text = "fps: " + str(Engine.get_frames_per_second())
