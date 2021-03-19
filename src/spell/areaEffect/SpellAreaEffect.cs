using System.Collections.Generic;
using Game.Actor;
using Game.Database;
using Godot;
using GC = Godot.Collections;
namespace Game.Ability
{
	public class SpellAreaEffect : Spell
	{
		private Area2D area;
		private readonly List<Character> targetedCharacters = new List<Character>();

		public override void _Ready()
		{
			base._Ready();

			area = GetChild<Area2D>(0);

			if (Globals.areaEffectDB.HasData(worldName))
			{
				CollisionShape2D sight = area.GetChild<CollisionShape2D>(0);
				((CircleShape2D)sight.Shape).Radius = Globals.areaEffectDB.GetData(worldName).radius;
			}
		}
		public override void Start()
		{
			base.Start();

			Character c;
			foreach (Area2D area2D in area.GetOverlappingAreas())
			{
				c = area2D.Owner as Character;
				if (c != null && c != character)
				{
					targetedCharacters.Add(c);
					StartAreaEffect(c);
				}
			}
		}
		public override void Exit()
		{
			base.Exit();
			targetedCharacters.ForEach(c => ExitAreaEffect(c));
		}
		protected virtual void StartAreaEffect(Character character)
		{
			Spell spell = new Spell();
			spell.Init(character, worldName);

			character.AddChild(spell);
			spell.Owner = character;

			spell.Start();
		}
		protected virtual void ExitAreaEffect(Character character) { }
		public override void Deserialize(GC.Dictionary payload)
		{
			// same as base method, but forces 'Start' on base class not here
			useCount = payload[NameDB.SaveTag.USE_COUNT_LEFT].ToString().ToInt();
			base.Start();
			useTimer.Start((float)payload[NameDB.SaveTag.USE_TIME_LEFT]);
			durTimer.Start((float)payload[NameDB.SaveTag.TIME_LEFT]);
		}
	}
}