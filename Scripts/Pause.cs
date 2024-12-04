using Godot;
using System;

public partial class Pause : Control
{
	
	public override void _Ready()
	{
		Visible = false; //La pausa obviamente inicia en off
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{
			GD.Print("pausado");

			bool isPaused = GetTree().Paused;
			GetTree().Paused = !isPaused;

			SetProcessInput(true);

			// Alterna la visibilidad del menú de pausa
			Visible = GetTree().Paused;
		}
	}
	
	private void _on_button_pressed(){
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
	
	private void _on_button_2_pressed(){
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
	}
}
