using Godot;
using System;


public partial class LLavecita : Area2D
{

	
	public void _on_body_entered (Node2D body){
		GD.Print("Llavesit pami");
		QueueFree();
	}	
}
