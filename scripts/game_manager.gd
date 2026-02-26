extends Node

const animal = preload("res://scenes/animal.tscn")
const pop_sfx = preload("res://assets/SFX/pop.ogg")
var audioStream: AudioStreamPlayer2D

var last_size: Vector2i
var button_press_cooldown = 1
var last_instantiation = 0

func _ready() -> void:
	print_debug('game started')
	audioStream = %AudioStreamPlayer2D
	
	var floor = get_parent().find_child("Floor") as Node2D
	if floor != null:
		floor.global_position = Vector2(50, get_viewport().get_visible_rect().size.y)
		floor.scale.x = 10			
	else:
		print_debug('floor not found')
	
	last_size = DisplayServer.window_get_size()

	
	
func _process(delta: float) -> void:
	last_instantiation -= .1
	if last_size != DisplayServer.window_get_size():
		var floor = get_parent().find_child("Floor") as Node2D
		if floor != null:
			floor.global_position = Vector2(50, get_viewport().get_visible_rect().size.y)
			floor.scale.x = 10			
		else:
			print_debug('floor not found')
		

func _input(event: InputEvent) -> void:	
	#print_debug('input even ' + event.as_text());
	if Input.is_anything_pressed():
		#if event is not InputEventKey:
			#pass
		#else:
			if last_instantiation > 0:
				pass
			else:
				var instance = animal.instantiate();
				instance.global_position = Vector2(randi_range(0, get_viewport().get_visible_rect().size.x), randi_range(0, get_viewport().get_visible_rect().size.y));
				
				add_child(instance)
								
				audioStream.pitch_scale = randf_range(0.5, 1.5)
				audioStream.play()
				last_instantiation = 0.7

			
	
