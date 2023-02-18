using System.Collections.Generic;
using System;
using Godot;
using GC = Godot.Collections;
using Game.Database;
namespace Game.Actor.Doodads
{
	public class CombatTextHandler : Node2D, ISerializable
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
		public bool ShouldSerialize() { return Serialize().Count > 0; }
		public GC.Dictionary Serialize()
		{
			GC.Array combatTextArr = new GC.Array();
			foreach (CombatText combatText in combatTexts.ToArray())
			{
				combatTextArr.Add(combatText.Serialize());
			}

			GC.Dictionary payload = new GC.Dictionary();
			if (combatTextArr.Count > 0)
			{
				payload[NameDB.SaveTag.COMBAT_TEXT] = combatTextArr;
			}

			return payload;
		}
		public void Deserialize(GC.Dictionary payload)
		{
			if (payload.Count == 0)
			{
				return;
			}

			GC.Array combatTextArr = (GC.Array)payload[NameDB.SaveTag.COMBAT_TEXT],
				combatTextPosArr;
			CombatText combatText;

			foreach (GC.Dictionary combatTextData in combatTextArr)
			{
				combatTextPosArr = (GC.Array)combatTextData[NameDB.SaveTag.POSITION];
				combatText = SceneDB.combatText.Instance<CombatText>();

				AddChild(combatText);
				combatText.Init(combatTextData[NameDB.SaveTag.TEXT].ToString(),
					(CombatText.TextType)Enum.Parse(typeof(CombatText.TextType),
						combatTextData[NameDB.SaveTag.STATE].ToString()),
					new Vector2((float)combatTextPosArr[0], (float)combatTextPosArr[1])
				);

				AddCombatText(combatText);
				combatText.Deserialize(combatTextData);
			}
		}
	}
}