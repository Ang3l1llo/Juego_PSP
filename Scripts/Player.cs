using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using NetHttpClient = System.Net.Http.HttpClient;

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

	private string playerId; // Para guardar el ID del jugador
	private string playerName; // Para guardar el nombre del jugador

	private string apiUrl = "https://api-psp-1nuc.onrender.com/api/player"; // URL de la API

	public bool isAttacking = false;
	public int Health = 100;
	public int Damage = 10;
	public int Level = 1;

	// Usamos HttpClient con alias(para evitar referencia ambigua)para todas las solicitudes HTTP en C#
	private NetHttpClient httpClient;

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

		// Inicializamos el HttpClient
		httpClient = new NetHttpClient();

		// Busca un nombre disponible y crea el jugador en la API
		BuscarNombreDisponible();
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

			sword.CollisionLayer = 3; // Asegurar el hit
			sword.CollisionMask = 2;

			if (IsOnFloor())
			{
				velocity.X = 0; // Detener el movimiento horizontal al atacar
			}
		}

		// Detectar si la animación de ataque ha terminado
		if (isAttacking && animatedSprite2D.Animation == "attack" && !animatedSprite2D.IsPlaying())
		{
			isAttacking = false;
			sword.CollisionLayer = 0;
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

	public void AddSword()
	{
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

		// Enviar la puntuación a la API
		SendScoreToAPI(playerId, enemiesKilled);
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
		GetTree().CallDeferred("change_scene_to_file", "res://Scenes/menu_muerte.tscn");
	}

	// Verifica si el nombre del jugador está disponible consultando la API
	private async void BuscarNombreDisponible()
	{
		int contador = 1;
		string nombreJugador = "Jugador1";
		bool nombreDisponible = false;

		while (!nombreDisponible)
		{
			nombreDisponible = await VerificarNombreDisponible(nombreJugador);
			if (!nombreDisponible)
			{
				contador++;
				nombreJugador = $"Jugador{contador}";
			}
		}

		// Crear el jugador automáticamente en la API
		CrearJugadorEnAPI(nombreJugador);
	}

	private async Task<bool> VerificarNombreDisponible(string nombre)
	{
		try
		{
			HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
			if (response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				var jugadores = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responseBody);
				foreach (var jugador in jugadores)
				{
					if (jugador["nombre"].ToString() == nombre)
					{
						return false; // El nombre ya existe
					}
				}
			}
			return true; // El nombre está disponible
		}
		catch (Exception ex)
		{
			GD.Print("Error en VerificarNombreDisponible: " + ex.Message);
			return true;
		}
	}

	private async void CrearJugadorEnAPI(string nombreJugador)
	{
		string url = apiUrl;
		var jsonData = new { Nombre = nombreJugador, Puntuacion = 0 };
		string jsonString = JsonSerializer.Serialize(jsonData);
		var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
		try
		{
			HttpResponseMessage response = await httpClient.PostAsync(url, content);
			if (response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
				playerId = responseData["id"].ToString();
				playerName = nombreJugador;
				GD.Print($"Jugador creado con ID: {playerId} y nombre: {playerName}");
			}
			else
			{
				GD.Print("Error al crear el jugador en la API. Código: " + response.StatusCode);
			}
		}
		catch (Exception ex)
		{
			GD.Print("Error en la solicitud HTTP: " + ex.Message);
		}
	}

	private async void SendScoreToAPI(string playerId, int score)
	{
		string url = apiUrl + "/" + playerId + "/addpoints";
		
		string jsonString = JsonSerializer.Serialize(score);
		var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
		try
		{
			HttpResponseMessage response = await httpClient.PutAsync(url, content);
			if (response.IsSuccessStatusCode)
			{
				GD.Print("Puntuación enviada correctamente.");
			}
			else
			{
				GD.Print($"Error al enviar la puntuación. Código: {response.StatusCode}");
			}
		}
		catch (Exception ex)
		{
			GD.Print("Error en la solicitud HTTP: " + ex.Message);
		}
	}
}
