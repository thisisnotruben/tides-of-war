using Godot;
namespace Game.Actor.Doodads
{
	public class BuffAnim : Particles2D
	{
		private Color color = Color.ColorN("white", 0.0f);

		public override void _Ready() { Modulate = color; }
		public BuffAnim Init(string itemName)
		{
			itemName = itemName.ToLower();

			if (itemName.Contains("healing"))
			{
				color = Color.ColorN("red");
			}
			else if (itemName.Contains("mana"))
			{
				color = Color.ColorN("blue");
			}
			else if (itemName.Contains("stamina"))
			{
				color = Color.ColorN("brown");
			}
			else if (itemName.Contains("intellect"))
			{
				color = Color.ColorN("purple");
			}
			else if (itemName.Contains("agility"))
			{
				color = Color.ColorN("green");
			}
			else if (itemName.Contains("strength"))
			{
				color = Color.ColorN("yellow");
			}
			else if (itemName.Contains("defense"))
			{
				color = Color.ColorN("gray");
			}

			return this;
		}
	}
}