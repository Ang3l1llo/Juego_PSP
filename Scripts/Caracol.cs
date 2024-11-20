using Godot;
using System;

public partial class Caracol : CharacterBody2D
{
	[Export] public int Speed = 25;
	[Export] public int Damage = 10;
	[Export] public int Health = 30;
	[Export] public float DetectionRange = 200.0f;

	private AnimatedSprite2D animatedSprite;
	private Vector2 _velocity;  
	private Node2D player;  
	private Timer deathTimer;

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		player = GetNode<Node2D>("/root/Game/Player"); 
		deathTimer = GetNode<Timer>("Timer");
		deathTimer.Timeout += TimeOut;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Aplicar gravedad 
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		else
		{
			velocity.Y = 0; 
		}

		// Movimiento horizontal hacia el jugador
		if (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) <= DetectionRange)
		{
			float directionX = (player.GlobalPosition.X - GlobalPosition.X) > 0 ? 1 : -1;
			velocity.X = directionX * Speed;
			
			var animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			animatedSprite.FlipH = directionX > 0;
		}
		else
		{
			velocity.X = 0; 
		}

		Velocity = velocity; 
		MoveAndSlide();      
	}

	public void TakeDamage(int damage)
	{
		Health -= damage;
		GD.Print($"Caracol took {damage} damage. Health is now {Health}.");
		if (Health <= 0)
		{
			Speed = 0;
			animatedSprite.Play("death");
			deathTimer.Start();
			 
		}	
	}
	public void TimeOut()
	{
		QueueFree();  
	}
}
