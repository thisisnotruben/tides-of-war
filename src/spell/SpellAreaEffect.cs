using System.Collections.Generic;
using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Ability
{
	public class SpellAreaEffect : SpellProto
	{
		private Area2D area;
		private CollisionShape2D sight;
		private readonly List<Character> targetedCharacters = new List<Character>();

		public SpellAreaEffect(Character character, string worldName) : base(character, worldName) { }
		public override void _Ready()
		{
			base._Ready();
			area = GetNode<Area2D>("area");
			sight = area.GetNode<CollisionShape2D>("sight");

			if (AreaEffectDB.HasAreaEffect(worldName))
			{
				((CircleShape2D)sight.Shape).Radius = AreaEffectDB.GetAreaEffect(worldName).radius;
			}
		}
		public void OnCharacterEntered(Area2D area2D)
		{
			Character character = area2D.Owner as Character;
			if (character != null)
			{
				targetedCharacters.Add(character);
			}
		}
		public void OnCharacterExited(Area2D area2D) { targetedCharacters.Remove(area2D.Owner as Character); }
		public override void Start()
		{
			base.Start();

			// lock in characters
			area.Monitoring = false;
			// don't want to double cast on primary target
			targetedCharacters.Remove(character);

			targetedCharacters.ForEach(c => StartAreaEffect(c));
		}
		public override void Exit()
		{
			base.Exit();
			targetedCharacters.ForEach(c => ExitAreaEffect(c));
		}
		protected virtual void StartAreaEffect(Character character)
		{
			SpellProto spell = new SpellProto(character, worldName);

			character.AddChild(spell);
			spell.Owner = character;

			spell.Start();
		}
		protected virtual void ExitAreaEffect(Character character) { }
	}
}