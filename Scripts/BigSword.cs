using Godot;
using System;

public partial class BigSword : Area2D
{
	private bool isPickedUp = false;
	
	public override void _Ready()
	{
		this.BodyEntered += _on_body_entered;
	}

	public void _on_body_entered(Node2D body)
	{
		if (body is Player player && Visible && !isPickedUp)
		{
			GD.Print("Espada recogida");
			player.AddToInventory("Sword"); 
			isPickedUp = true;
			QueueFree(); 
			player.AddSword();
			player.Damage+=10;
		}
	}
}
