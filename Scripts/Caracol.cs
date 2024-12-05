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
	private bool isHiding = false; 
	private bool isDead = false;
	private Timer hideTimer; 
	private Timer stepTimer; 

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		player = GetNode<Node2D>("/root/Game/Player"); 
		deathTimer = GetNode<Timer>("DeathTimer");
		deathTimer.Timeout += TimeOut;
		damageArea = GetNode<Area2D>("DamageArea");
		hideTimer = GetNode<Timer>("HideTimer"); 
		hideTimer.Timeout += ExitHideState;

		// Configurar el temporizador de los pasitos aleatorios
		stepTimer = GetNode<Timer>("StepTimer");
		stepTimer.Timeout += TakeStep;
		stepTimer.Start(2.0f); // Intervalo para los pasitos
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
			// Movimiento hacia el jugador dentro del rango
			if (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) <= DetectionRange && !isHiding)
			{
				float directionX = (player.GlobalPosition.X - GlobalPosition.X) > 0 ? 1 : -1;
				velocity.X = directionX * Speed;

				animatedSprite.FlipH = directionX > 0;
				
				damageArea.Position = new Vector2(directionX > 0 ? 30 : 2, damageArea.Position.Y);
			}
		}

		Velocity = velocity; 
		MoveAndSlide();      
	}

	private void TakeStep()
	{
		// Si el caracol está escondido o cerca del jugador, no hacer pasos aleatorios
		if (isHiding || (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) <= DetectionRange))
			return;

		// Movimiento de pasos pequeños hacia la izquierda o derecha
		int direction = GD.Randf() > 0.5f ? 1 : -1;
		Velocity = new Vector2(direction * Speed, 0); 
		animatedSprite.FlipH = direction > 0;
		animatedSprite.Play("idle");

		// Restablecer velocidad después de un tiempo corto
		Timer resetVelocityTimer = new Timer();
		AddChild(resetVelocityTimer);
		resetVelocityTimer.OneShot = true;
		resetVelocityTimer.WaitTime = 0.5f;
		resetVelocityTimer.Timeout += () =>
		{
			if (!isHiding && (player == null || GlobalPosition.DistanceTo(player.GlobalPosition) > DetectionRange))
			{
				Velocity = Vector2.Zero; 
				animatedSprite.Play("idle");
			}
		};
		resetVelocityTimer.Start();
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
		}
		else
		{
			// Si aún no ha muerto, realizar el retroceso y esconderse
			animatedSprite.Play("hide");
			knockBack = hitDirection * 20; 
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

	private void TimeOut()
	{
		if(isDead) return;
		isDead = true;
		var playerr = GetNode<Player>("/root/Game/Player");
		playerr.OnEnemyKilled();
		QueueFree();  
	}
}
