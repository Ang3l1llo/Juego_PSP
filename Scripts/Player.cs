using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 150.0f;
	public const float JumpVelocity = -370.0f;
	private AnimatedSprite2D animatedSprite2D;
	private Timer deathTimer;
	private Vector2 knockBack;

	// Variable para manejar si está atacando
	public bool isAttacking = false;
	
	public int Health = 100;
	public int Damage = 10;
	public int Level = 1;

	public override void _Ready()
	{
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		var sword = GetNode<Sword>("Sword");
		sword.SetPlayer(this);
		deathTimer = GetNode<Timer>("Timer");
		deathTimer.Timeout += TimeOut;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Añadir gravedad si no está en el suelo
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		
		// Referencia a la espada
		var sword = GetNode<Sword>("Sword");
	
		// Manejar el ataque
		if (Input.IsActionJustPressed("attack") && !isAttacking)
		{
			isAttacking = true;
			animatedSprite2D.Play("attack");
			 
			sword.CollisionLayer = 3; //Tengo que hacerlo así para asegurar el hit porq daba errores
			sword.CollisionMask = 2;  
			
			if(IsOnFloor()){
				velocity.X = 0;// Detener el movimiento horizontal al atacar
			}
			  
		}

		// Detectar si la animación de ataque ha terminado
		if (isAttacking && animatedSprite2D.Animation == "attack" && !animatedSprite2D.IsPlaying())
		{
			isAttacking = false;
			 
			sword.CollisionLayer= 0; 
			sword.CollisionMask = 0;  
		}

		// Si no está atacando
		if (!isAttacking)
		{
			// Manejar el salto
			if (Input.IsActionJustPressed("jump") && IsOnFloor())
			{
				velocity.Y = JumpVelocity;
				animatedSprite2D.Play("jump");
			}
			else if (IsOnFloor())
			{
				if (knockBack.Length() > 0)
				{
					knockBack = knockBack.MoveToward(Vector2.Zero, 200 * (float)delta); 
					velocity += knockBack; 
				}
				// Manejar el movimiento en el eje X
				else if (Input.IsActionPressed("move_left"))
				{
				
					velocity.X = -Speed;
					animatedSprite2D.FlipH = true;
					sword.Position = new Vector2(-52, 0);
					animatedSprite2D.Play("run");
				}
				else if (Input.IsActionPressed("move_right"))
				{
					velocity.X = Speed;
					animatedSprite2D.FlipH = false;
					sword.Position = new Vector2(-4, 0);
					animatedSprite2D.Play("run");
				}
				else
				{
					velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
					animatedSprite2D.Play("idle");
				}
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	
	public void TakeDamage(int damage, Vector2 hitDirection)
	{
		Health -= damage;
		GD.Print($"Player took {damage} damage. Health is now {Health}.");
		
		knockBack = hitDirection * 40; 

		if (Health <= 0)
		{
			Die();
		}
	}
	
	private void Die()
	{
		GD.Print("Player has died.");
		animatedSprite2D.Play("death");
		SetProcess(false);
		SetPhysicsProcess(false);
		deathTimer.Start();
		
	}
	
	public void TimeOut()
	{
		if(IsInsideTree()){ //Para evitar el error de que se llame al nodo player cuando ya no esta asociado a la escena
			GetTree().ReloadCurrentScene();
		}
		 
		
	}
	
}
