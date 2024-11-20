using Godot;
using System;

public partial class Sword : Area2D
{
	private Player player; 

	public override void _Ready()
	{
		
	}
	
	public void SetPlayer(Player player)
	{
		this.player = player;
	}

	public void _on_body_entered(Node2D body)
	{
	
		if (body is Caracol enemy && player.isAttacking)
		{
			GD.Print("Sword hit enemy!");
			enemy.TakeDamage(player.Damage); // Hacer daño al enemigo
		}
	}
} 
