extends Node

var mpt: AudioStreamMPT

func _on_module_resource_set(player: AudioStreamPlayer, file_path: String):
	var mpt_stream = AudioStreamMPT.new()
	var file_data = FileAccess.get_file_as_bytes(file_path)
	mpt_stream.data = file_data
	player.stream = mpt_stream
	
