using Godot;
using System;

public partial class JabaliBoss : CharacterBody2D
{
	public int Speed = 15;
	public int RunSpeed = 50;
	public int Damage = 20;
	public int Health = 80;
	public float DetectionRange = 200.0f;

	private AnimatedSprite2D animatedSprite;
	private Node2D player;
	private Timer stepTimer;
	private Timer deathTimer;
	private Timer hitTimer;
	private Area2D damageArea; 
	private bool isRunning = false; 
	private bool isTakingDamage = false; 
	private bool isDead = false; 
	private Vector2 knockBack;

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		player = GetNode<Node2D>("/root/Game/Player");
		deathTimer = GetNode<Timer>("DeathTimer");
		deathTimer.Timeout += TimeOut;
		hitTimer = GetNode<Timer>("HitTimer"); 
		hitTimer.Timeout += GetHit; 
		damageArea = GetNode<Area2D>("DamageArea");
		damageArea.BodyEntered += OnBodyEntered;
		stepTimer = GetNode<Timer>("StepTimer");
		stepTimer.Timeout += TakeStep;
		stepTimer.Start(2.0f); // Comienza el temporizador para pequeños pasos
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isDead) return; // Para el control de animaciones

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

		// Reducir gradualmente el retroceso
		if (knockBack.Length() > 0)
		{
			knockBack = knockBack.MoveToward(Vector2.Zero, 200 * (float)delta);
			velocity += knockBack;
		}
		else if (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) <= DetectionRange)
		{
			// Modo de correr hacia el jugador
			if (!isTakingDamage) // No correr si está recibiendo daño
			{
				float directionX = (player.GlobalPosition.X - GlobalPosition.X) > 0 ? 1 : -1;
				velocity.X = directionX * RunSpeed;

				animatedSprite.FlipH = directionX > 0;
				animatedSprite.Play("run");

				// Ajustar posición del área de daño
				damageArea.Position = new Vector2(directionX > 0 ? 30 : 0, damageArea.Position.Y);
			}
		}
		
		Velocity = velocity;
		MoveAndSlide();
	}

	private void TakeStep()
	{
		if (isTakingDamage || player != null && GlobalPosition.DistanceTo(player.GlobalPosition) <= DetectionRange) return;

		// Movimiento de pasos pequeños hacia la izquierda o derecha
		int direction = GD.Randf() > 0.5f ? 1 : -1;
		Velocity = new Vector2(direction * Speed, 0); 
		animatedSprite.FlipH = direction > 0;
		animatedSprite.Play("walk");

		// Restablecer velocidad después de un tiempo corto
		Timer resetVelocityTimer = new Timer();
		AddChild(resetVelocityTimer);
		resetVelocityTimer.OneShot = true;
		resetVelocityTimer.WaitTime = 0.5f;
		resetVelocityTimer.Timeout += () =>
		{
			if (!isTakingDamage && (player == null || GlobalPosition.DistanceTo(player.GlobalPosition) > DetectionRange))
			{
				Velocity = Vector2.Zero; 
			}
		};
		resetVelocityTimer.Start();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			Vector2 hitDirection = (player.GlobalPosition - GlobalPosition).Normalized();
			GD.Print("Jabalí hizo daño al jugador.");
			player.TakeDamage(Damage, hitDirection);
		}
	}

	public void TakeDamage(int damage, Vector2 hitDirection)
	{
		if (isTakingDamage || isDead) return; // Ignorar si ya está recibiendo daño o está muerto

		Health -= damage;
		GD.Print($"Jabalí took {damage} damage. Health is now {Health}.");

		if (Health <= 0) 
		{
			// Detener todo movimiento y animaciones
			isDead = true; // Marcar que el jabalí está muerto
			Speed = 0; 
			Velocity = Vector2.Zero;
			animatedSprite.Play("hit");
			deathTimer.Start(); // Iniciar temporizador de muerte
		}
		else
		{
			// Retroceso y animación de ser golpeado
			knockBack = hitDirection * 23; // Ajustar retroceso más controlado
			Speed = 0; 
			isTakingDamage = true; 
			animatedSprite.Play("hit");
			hitTimer.Start(1.0f); // Retrasar el retorno al movimiento
		}
	}

	private void GetHit()
	{
		isTakingDamage = false; 
		Speed = 50; 
		animatedSprite.Play("run"); 
	}

	private void TimeOut()
	{
		QueueFree(); 
	}
}
