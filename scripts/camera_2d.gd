extends Camera2D

func _ready():
	var background_size = get_child(0).get_child(0).texture.get_size()
	var viewport_size = get_viewport().size as Vector2
	zoom = viewport_size / background_size
