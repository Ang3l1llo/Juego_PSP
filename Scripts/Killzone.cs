using Godot;
using System;

public partial class Killzone : Area2D
{
	public void _on_body_entered (Node2D body){
		GD.Print("DEAD");
		GetTree().ReloadCurrentScene();
	}
}
