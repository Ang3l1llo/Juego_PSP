using Godot;
using System;

public partial class MenuVictoria : Control
{
	private void _on_button_pressed(){
		GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
	}
	
	private void _on_button_2_pressed(){
		GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
	}
	
	private void _on_button_3_pressed(){
		GetTree().ChangeSceneToFile("res://Scenes/RankingScreen.tscn");
	}
}
