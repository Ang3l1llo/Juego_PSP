using Godot;
using System;

public partial class Cofre : Area2D
{
	private AnimatedSprite2D animatedSprite;
	private bool open = false;
	private Node2D sword;
	private Timer swordTimer;

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sword = GetNode<Node2D>("BigSword");
		sword.Visible = false;
		
		swordTimer = new Timer();
		AddChild(swordTimer);
		swordTimer.WaitTime = 2.0f; 
		swordTimer.OneShot = true;
		swordTimer.Timeout += ShowSword; 
	}

	public void _on_body_entered(Node2D body)
	{
		if (body is Player player) 
		{
			if (player.HasItem("key") && !open) 
			{
				open = true;
				GD.Print("Cofresito lindo se abre");
				animatedSprite.Play("animation");
				player.Inventory.Remove("Key");
				swordTimer.Start();
			}
			else
			{
				if(!open){
					GD.Print("Necesitas una llave para abrir el cofre.");
				}
				
			}
		}
	}
	private void ShowSword()
	{
		sword.Visible = true;
		GD.Print("Toma espadita");
	}
}
