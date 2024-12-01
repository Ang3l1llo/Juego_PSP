using Godot;
using System;

public partial class Caracol : CharacterBody2D
{
	public int Speed = 25;
	public int Damage = 10;
	public int Health = 30;
	public float DetectionRange = 200.0f;

	private AnimatedSprite2D animatedSprite;
	private Vector2 knockBack;  
	private Node2D player;  
	private Timer deathTimer;
	private Area2D damageArea;
	private bool isHiding = false; // Estado para controlar si está escondido
	private Timer hideTimer; // Temporizador para salir del estado "hide"


	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		player = GetNode<Node2D>("/root/Game/Player"); 
		deathTimer = GetNode<Timer>("Timer");
		deathTimer.Timeout += TimeOut;
		damageArea = GetNode<Area2D>("DamageArea");
		hideTimer = GetNode<Timer>("HideTimer"); 
		hideTimer.Timeout += ExitHideState; 
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

	// Reducir gradualmente la velocidad del retroceso
	if (knockBack.Length() > 0)
	{
		knockBack = knockBack.MoveToward(Vector2.Zero, 200 * (float)delta); 
		velocity += knockBack; 
	}
	else
	{
		// Movimiento normal solo si no hay retroceso
		if (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) <= DetectionRange)
		{
			float directionX = (player.GlobalPosition.X - GlobalPosition.X) > 0 ? 1 : -1;
			velocity.X = directionX * Speed;

			animatedSprite.FlipH = directionX > 0;
			
			damageArea.Position = new Vector2(directionX > 0 ? 30 : 2, damageArea.Position.Y);
			
			
		}
		else
		{
			velocity.X = 0; 
		}
	}

	Velocity = velocity; 
	MoveAndSlide();      
	}
	
	private void OnBodyEntered(Node2D body)
{
	if (body is Player player)
	{
		// Calcular la dirección del golpe
		Vector2 hitDirection = (player.GlobalPosition - GlobalPosition).Normalized();
		GD.Print("Caracol hizo daño al jugador.");
		player.TakeDamage(Damage, hitDirection); 
	}
}


	public void TakeDamage(int damage, Vector2 hitDirection)
	{
		if (isHiding) return; // Si ya está escondido, ignorar más daño

		Health -= damage;
		GD.Print($"Caracol took {damage} damage. Health is now {Health}.");

   		if (Health <= 0) 
		{
			Speed = 0; 
			animatedSprite.Play("death");
			Velocity = Vector2.Zero;
			deathTimer.Start();
			
		}else{
			// Si aún no ha muerto, realizar el retroceso y esconderse
			animatedSprite.Play("hide");
			knockBack = hitDirection * 40; 
			Speed = 0; 
			isHiding = true; 
			hideTimer.Start(1.5f); 
		}
	}

	private void ExitHideState()
	{
		isHiding = false; 
		Speed = 25; 
		animatedSprite.Play("idle"); 
	}
	public void TimeOut()
	{
		QueueFree();  
	}
}
