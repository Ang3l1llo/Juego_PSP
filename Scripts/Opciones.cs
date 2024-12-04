using Godot;
using System;

public partial class Opciones : Control
{
	private void _on_button_pressed(){
		GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
	}
}
