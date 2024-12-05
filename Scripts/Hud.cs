using Godot;
using System;

public partial class Hud : CanvasLayer
{
	private TextureProgressBar healthBar;
	private Sprite2D sword;
	
	public override void _Ready()
	{
		healthBar = GetNode<TextureProgressBar>("Control/healthBar");
		sword = GetNode<Sprite2D>("Control/SwordSprite");
		sword.Visible = false;
	}

	public void UpdateHealth(int health)
	{
		healthBar.Value = health;
	}
	
	public void ShowSword()
	{
		sword.Visible = true;
	}
	
	public void UpdateEnemyCount(int remainingEnemies)
	{
		var enemyCounter = GetNode<Label>("Control/EnemyCounter");
		enemyCounter.Text = $"Enemies: {remainingEnemies}";
	}
}
