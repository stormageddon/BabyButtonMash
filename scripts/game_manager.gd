extends Node

const animal = preload("res://scenes/animal.tscn")
const pop_sfx = preload("res://assets/SFX/pop.ogg")
var audioStream: AudioStreamPlayer2D

var last_size: Vector2i
var button_press_cooldown = 1
var last_instantiation = 0
var key_blocker_pid: int = 0  # PID of AltTabBlocker process when running (0 = not running)

func _ready() -> void:
	print_debug('game started')
	add_to_group("game_manager")  # so LockButton can find this node
	#_install_key_blocker()
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

func install_key_blocker() -> void:
	if OS.get_name() != "Windows":
		return
	if key_blocker_pid != 0:
		return  # already running
	var blocker_path: String = _find_blocker_exe()
	if blocker_path.is_empty():
		push_warning("KeyBlocker: AltTabBlocker.exe not found")
		return
	var args: PackedStringArray = PackedStringArray(["--attach", str(OS.get_process_id())])
	# false = no console window, so no flicker when we kill the process on unlock
	var pid: int = OS.create_process(blocker_path, args, false)
	if pid == 0:
		push_warning("KeyBlocker: failed to start AltTabBlocker.exe (pid=0)")
	else:
		key_blocker_pid = pid
		print_debug("KeyBlocker: started AltTabBlocker.exe pid=%s" % pid)

func uninstall_key_blocker() -> void:
	if key_blocker_pid == 0:
		return
	var err: Error = OS.kill(key_blocker_pid)
	if err == OK:
		print_debug("KeyBlocker: stopped AltTabBlocker.exe pid=%s" % key_blocker_pid)
	key_blocker_pid = 0

func _find_blocker_exe() -> String:
	var name: String = "AltTabBlocker.exe"
	# 1) Same folder as the running executable (exported game)
	var exe_path: String = OS.get_executable_path()
	if not exe_path.is_empty():
		var p: String = exe_path.get_base_dir().path_join(name)
		if FileAccess.file_exists(p):
			return p
	# 2) Project root (when running from editor or exe in subfolder)
	var res_path: String = ProjectSettings.globalize_path("res://")
	if not res_path.is_empty():
		var project_dir: String = res_path.trim_suffix("/")
		var p: String = project_dir.path_join(name)
		if FileAccess.file_exists(p):
			return p
	return ""

