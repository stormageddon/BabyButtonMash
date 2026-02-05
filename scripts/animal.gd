extends Node2D

var sprites = ["bear","buffalo","chick","chicken","cow","crocodile","dog","duck","elephant","frog","giraffe","goat","gorilla","hippo","horse","monkey","moose","narwhal","owl","panda","parrot","penguin","pig","rabbit","rhino","sloth","snake","walrus","whale","zebra"]
var rb: RigidBody2D
var movement: Vector2
var audioStream: AudioStreamPlayer2D

func _ready() -> void:
	var sprite = find_child("Sprite2D") as Sprite2D
	sprite.texture = load("res://assets/" + sprites[randi_range(0, sprites.size() - 1)] + ".png");
	rb = find_child("RigidBody2D")
	#connect("body_entered", _on_body_entered) 
	audioStream = find_child("AudioStreamPlayer2D")
	
func _input(event):
	pass

func _process(delta):
	pass
	
func _physics_process(delta: float) -> void:
	pass

func _on_rigid_body_2d_mouse_entered() -> void:	
	var v = Vector2.UP * 150 * Input.get_last_mouse_velocity()
	rb.apply_force(v)
	audioStream.pitch_scale = randf_range(0.5, 1.5)
	audioStream.play()
	 
	
