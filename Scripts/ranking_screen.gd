extends Control

const API_URL = "http://api-psp-1nuc.onrender.com/api/player/top5"

@onready var ranking_container = $VBoxContainer

func _ready():
	fetch_top_players()

func fetch_top_players():
	var http_request = HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(_on_request_completed)
	
	var error = http_request.request(API_URL)
	if error != OK:
		print("Error al hacer la solicitud:", error)

func _on_request_completed(_result, response_code, _headers, body):
	if response_code == 200:
		var json = JSON.parse_string(body.get_string_from_utf8())
		if json:
			print("Respuesta de la API:", json)
			update_ranking(json)
	else:
		print("Error al obtener los jugadores:", response_code)

func update_ranking(players):
	for child in ranking_container.get_children():
		ranking_container.remove_child(child)
		child.queue_free()

	for player in players:
		var label = Label.new()
		label.text = "%s - %d puntos" % [player["nombre"], player["puntuacion"]]
		ranking_container.add_child(label)

func _on_button_2_pressed():
	get_tree().change_scene_to_file("res://Scenes/menu.tscn")
