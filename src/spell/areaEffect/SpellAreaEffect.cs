using System.Collections.Generic;
using Game.Actor;
using Game.Database;
using Game.Factory;
using Game.GameItem;
using Godot;
namespace Game.Ability
{
	public class SpellAreaEffect : Spell
	{
		private Area2D area;
		private readonly List<Character> targetedCharacters = new List<Character>();
		private readonly SpellFactory spellFactory = new SpellFactory();

		public override void _Ready()
		{
			base._Ready();

			area = GetNode<Area2D>("area");
			CollisionShape2D sight = area.GetChild<CollisionShape2D>(0);


			// if (AreaEffectDB.HasAreaEffect(worldName))
			// {
			// 	sight.Shape.SetDeferred("radius", AreaEffectDB.GetAreaEffect(worldName).radius);
			// }

			area.Connect("area_entered", this, nameof(OnCharacterEntered));
			area.Connect("area_exited", this, nameof(OnCharacterExied));
		}
		public void OnCharacterEntered(Area2D area2D)
		{
			Character c = area2D.Owner as Character;
			if (c != null && c != character)
			{
				targetedCharacters.Add(c);
			}
		}
		public void OnCharacterExied(Area2D area2D) { targetedCharacters.Remove(area2D.Owner as Character); }
		public override void Start()
		{
			base.Start();
			targetedCharacters.ForEach(c => StartAreaEffect(c));
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
	}
}