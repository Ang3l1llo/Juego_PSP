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

	public void onBodyEntered(Node2D body)
	{
	
		if (body is Caracol caracol && player.isAttacking)
		{
			Vector2 hitDirection = (caracol.GlobalPosition - player.GlobalPosition).Normalized(); //Para el parámetro del retroceso
			GD.Print("Sword hit enemy!");
			caracol.TakeDamage(player.Damage, hitDirection); 
		}else if (body is Jabali jabali && player.isAttacking) 
		{
			Vector2 hitDirection = (jabali.GlobalPosition - player.GlobalPosition).Normalized(); 
			GD.Print("Sword hit Jabali!");
			jabali.TakeDamage(player.Damage, hitDirection); 
		}else if (body is JabaliBoss jabaliboss && player.isAttacking)
		{
			Vector2 hitDirection = (jabaliboss.GlobalPosition - player.GlobalPosition).Normalized(); 
			GD.Print("Sword hit Jabali!");
			jabaliboss.TakeDamage(player.Damage, hitDirection);
		}
	}
} 
