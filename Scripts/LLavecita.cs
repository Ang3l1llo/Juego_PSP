using Godot;
using System;


public partial class LLavecita : Area2D
{

	
	public void _on_body_entered (Node2D body){
		
		if (body is Player player) 
		{
			GD.Print("Llavesita pami");
			player.AddToInventory("key"); 
			QueueFree(); 
		}
	}	
}
