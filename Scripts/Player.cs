using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	public const float Speed = 150.0f;
	public const float JumpVelocity = -370.0f;
	private AnimatedSprite2D animatedSprite2D;
	private Timer deathTimer;
	private Vector2 knockBack;
	private TextureProgressBar healthBar;
	public List<string> Inventory = new List<string>();
	private Hud hud;
	private int enemiesToKill = 8;
	private int enemiesKilled = 0;
	
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
		healthBar = GetNode<TextureProgressBar>("../HUD/Control/healthBar");
		healthBar.Value = Health;
		hud = GetNode<Hud>("../HUD");
		hud.UpdateEnemyCount(enemiesToKill - enemiesKilled);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		healthBar.Value = Health;

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
				velocity.X = 0;// Detener el movimiento horizontal al atacar, se veia raro
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
					sword.Position = new Vector2(-45, 0);
					animatedSprite2D.Play("run");
				}
				else if (Input.IsActionPressed("move_right"))
				{
					velocity.X = Speed;
					animatedSprite2D.FlipH = false;
					sword.Position = new Vector2(1, 0);
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
	public void AddToInventory(string item)
	{
		Inventory.Add(item); 
		GD.Print($"Item añadido al inventario: {item}");
	}
	
	public void AddSword(){
		hud.ShowSword();
	}

	public bool HasItem(string item)
	{
		return Inventory.Contains(item); 
	}
	
	public void OnEnemyKilled()
	{
		enemiesKilled++;
		hud.UpdateEnemyCount(enemiesToKill - enemiesKilled);

		if (enemiesKilled >= enemiesToKill)
		{
			ShowVictoryMenu();
		}
	}
	
	private void ShowVictoryMenu()
	{
		GD.Print("¡Has ganado!");
		GetTree().ChangeSceneToFile("res://Scenes/menu_victoria.tscn");
		
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
		GetTree().CallDeferred("change_scene_to_file", "res://Scenes/menu_muerte.tscn");//Sin usar el calldeferred me daba un error, solucionado gracias a nuestro querido delegado
	}
}
