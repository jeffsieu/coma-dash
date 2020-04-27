extends Collectible

class_name WeaponCollectible

var TripleGun = preload("res://Weapon/TripleGun/TripleGun.gd")

const TYPE: int = Collectible.WEAPON

const duration: float = 3.0

var total_delta: float = 0.0
var weapon: GDScript = TripleGun

func _ready() -> void:
	._ready()
	self.connect('collected', _player, 'on_weapon_collected')
	self.connect('collected', self, 'on_collected')


func on_collected(collectible) -> void:
	_player.on_weapon_collected(self)
