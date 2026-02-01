extends Node

@export var enabled : bool = true
@export var mpt_volume_db : float = 5.0

var parent : AudioStreamPlayer

func _ready():
	if not enabled:
		process_mode = Node.PROCESS_MODE_DISABLED
		return
		
	parent = get_parent() as AudioStreamPlayer

func _physics_process(delta):
	on_stream_changed()

func on_stream_changed():
	if parent.stream is AudioStreamMPT:
		parent.volume_db = mpt_volume_db
	else:
		parent.volume_db = 0.0
