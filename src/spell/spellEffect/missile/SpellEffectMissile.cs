using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Ability
{
	public class SpellEffectMissile : SpellEffect
	{
		public override void Init(Character character, string spellWorldName)
		{
			base.Init(character, spellWorldName);

			switch (spellWorldName)
			{
				case WorldNameDB.ARCANE_BOLT:
				case WorldNameDB.FIREBALL:
				case WorldNameDB.FROST_BOLT:
				case WorldNameDB.METEOR:
				case WorldNameDB.SHADOW_BOLT:
				case WorldNameDB.SIPHON_MANA:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Node2D bolt = idleParticles.GetNode<Node2D>("bolt");
						tween.InterpolateProperty(bolt, ":modulate", bolt.Modulate,
							new Color("00ffffff"), lightFadeDelay,
							Tween.TransitionType.Linear, Tween.EaseType.In);
						tween.Start();
						timer.Start();

						if (spellWorldName.Equals(WorldNameDB.SIPHON_MANA))
						{
							GlobalPosition = character.target.pos;
						}
					};
					break;
				case WorldNameDB.CONCUSSIVE_SHOT:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						light.Show();
						PackedScene bashScene = (PackedScene)GD.Load("res://src/spell/effects/bash.tscn");
						// bash_effect bash = (bash_effect)bashScene.Instance();
						// character.target.AddChild(bash);
						// bash.Owner = character.target;
						// spell.Connect(nameof(Unmake), bash, nameof(bash_effect._OnTimerTimeout));
						// bash.OnHit();
						tween.Start();
						timer.Start();
					};
					break;
				case WorldNameDB.EXPLOSIVE_ARROW:
				case WorldNameDB.SNIPER_SHOT:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						light.Show();
						tween.Start();
						timer.Start();
					};
					break;
				case WorldNameDB.MIND_BLAST:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						// Bolt bolt = Owner as Bolt;
						// if (bolt != null)
						// {
						// 	bolt.GlobalPosition = bolt.target.head.GlobalPosition;
						// 	tween.Start();
						// 	timer.Start();
						// }
						// else
						// {
						// 	GD.Print("Owner not bolt in class MindBlast");
						// }
					};
					break;
				case WorldNameDB.PIERCING_SHOT:
				case WorldNameDB.PRECISE_SHOT:
				case WorldNameDB.SEARING_ARROW:
				case WorldNameDB.STINGING_SHOT:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						tween.Start();
						timer.Start();
					};
					break;
				case WorldNameDB.SLOW:
					onHitEffect = () =>
					{
						// base._OnTimerTimeout();
						// Bolt bolt = Owner as Bolt;
						// if (bolt != null)
						// {
						// 	GetParent().RemoveChild(this);
						// 	bolt.target.AddChild(this);
						// 	Position = bolt.target.img.Position;
						// 	spell.Connect(nameof(Unmake), this, nameof(_OnTimerTimeout));
						// 	tween.Start();
						// 	timer.Start();
						// }
						// else
						// {
						// 	GD.Print("Unexpected parent in class: Slow");
						// }
					};
					onTimeOut = () =>
					{
						// base._OnTimerTimeout();
						FadeLight(true);
						foreach (Particles2D particles2D in idleParticles.GetChildren())
						{
							particles2D.Emitting = false;
						}
						timer.Start();
					};
					break;
			}
		}
	}
}