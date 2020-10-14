using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Ability
{
	public class SpellEffectTargetless : SpellEffect
	{
		public override void Init(Character character, string spellWorldName)
		{
			base.Init(character, spellWorldName);

			switch (spellWorldName)
			{
				case WorldNameDB.DIVINE_HEAL:
				case WorldNameDB.FORTIFY:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Position = character.head.Position;
						tween.Start();
						timer.Start();
					};
					onTimeOut = () =>
					{
						// base._OnTimerTimeout();
						FadeLight(true);
						tween.InterpolateProperty(this, ":modulate", Modulate,
							new Color("00ffffff"), lightFadeDelay,
							Tween.TransitionType.Linear, Tween.EaseType.InOut);
						tween.Start();
						timer.Start();
						foreach (Particles2D particles2D in idleParticles.GetChildren())
						{
							particles2D.Emitting = false;
						}
					};
					break;
				case WorldNameDB.FRENZY:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Position = character.head.Position;
						character.GetParent().MoveChild(this, 0);
						tween.Start();
						timer.Start();
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
				case WorldNameDB.HASTE:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Position = character.img.Position;
						tween.Start();
						timer.Start();
					};
					break;
				case WorldNameDB.INTIMIDATING_SHOUT:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Position = character.head.Position + new Vector2(0.0f, 6.0f);
						tween.Start();
						timer.Start();
					};
					onTimeOut = () =>
					{
						FadeLight(true);
						foreach (Particles2D particles2D in idleParticles.GetChildren())
						{
							particles2D.Emitting = false;
						}
						timer.Start();
					};
					break;
				case WorldNameDB.STOMP:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						tween.Start();
						timer.Start();
					};
					break;
			}
		}
	}
}