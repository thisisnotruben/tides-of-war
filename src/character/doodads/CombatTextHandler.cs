using System.Collections.Generic;
using Godot;
namespace Game.Actor.Doodads
{
	public class CombatTextHandler : Node2D
	{
		private readonly Queue<CombatText> combatTexts = new Queue<CombatText>();

		public void AddCombatText(CombatText combatText)
		{
			combatText.Connect(nameof(CombatText.MidwayThrough), this, nameof(OnMidwayThroughCombatText));
			combatText.Connect(nameof(CombatText.Finished), this, nameof(OnCombatTextFinished),
				new Godot.Collections.Array() { combatText });

			combatTexts.Enqueue(combatText);

			// else: it'll be queued in mid-animation sequence
			if (combatTexts.Count == 1)
			{
				combatText.Start();
			}
		}
		public void OnMidwayThroughCombatText()
		{
			combatTexts.Dequeue();
			StartNextCombatText();
		}
		public void OnCombatTextFinished(CombatText combatText)
		{
			combatText.QueueFree();
			StartNextCombatText();
		}
		private void StartNextCombatText()
		{
			if (combatTexts.Count > 0 && !combatTexts.Peek().anim.IsPlaying())
			{
				combatTexts.Peek().Start();
			}
		}
	}
}