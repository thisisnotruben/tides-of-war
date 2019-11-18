using System;
using System.Collections.Generic;
using Game.Actor;
using Game.Misc.Other;
using Game.Spell;
using Godot;
namespace Game.Misc.Missile
{
    public class Bolt : WorldObject
    {
        Vector2 spawnPos;
        bool rotate;
        bool reverse;
        bool hasHitTarget;
        bool instantSpawn;
        WorldTypes weaponType;
        WorldTypes swingType;
        Character target;
        Character originator;
        Spell.Spell spell;
        [Signal]
        public delegate void Hit(Spell.Spell spell);
        public override void _Ready()
        {
            SetProcess(false);
        }
        public override void _Process(float delta)
        {
            if (rotate && !hasHitTarget)
            {
                LookAt(target.GetCenterPos());
            }
            else if (reverse)
            {
                Move(target, originator);
            }
            else
            {
                Move(originator, target);
            }
        }
        public void _OnProjectileAreaEntered(Area2D area2D)
        {
            if (!hasHitTarget && area2D.GetOwner() == target)
            {
                EmitSignal(nameof(Hit), spell);
                SetZIndex(1);
                hasHitTarget = true;
                if (GetNode<Sprite>("img").GetTexture() != null)
                {
                    AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
                    if (spell != null)
                    {
                        anim.Play("img_fade");
                    }
                    else
                    {
                        anim.Play("fade");
                    }
                }
                if (!target.IsDead())
                {
                    Attack();
                }
            }
        }
        public void _OnAnimFinished(string animName)
        {
            if (animName.Equals("fade"))
            {
                SetProcess(false);
                GetNode<Tween>("tween").RemoveAll();
                QueueFree();
            }
        }
        private void Move(Character fromUnit, Character toUnit)
        {
            Tween tween = GetNode<Tween>("tween");
            tween.InterpolateProperty(this, ":global_position", GetGlobalPosition(), toUnit.GetCenterPos(),
                spawnPos.DistanceTo(toUnit.GetCenterPos()) / (float)fromUnit.GetWeaponRange(),
                Tween.TransitionType.Circ, Tween.EaseType.Out);
            tween.Start();
        }
        public void SetTarget(Character character)
        {
            target = character;
            spawnPos = originator.GetNode<Node2D>("img/missile").GetGlobalPosition();
            if (rotate)
            {
                LookAt(target.GetCenterPos());
            }
            if (instantSpawn)
            {
                SetGlobalPosition(target.GetCenterPos());
            }
            else
            {
                SetProcess(true);
            }
            Show();
        }
        public Character GetTarget()
        {
            return target;
        }
        public void SetSpell(Spell.Spell spell)
        {
            this.spell = spell;
        }
        public virtual void SetUp(Character originator, Vector2 globalPosition, Spell.Spell spell)
        {
            SetSpell(spell);
            spawnPos = globalPosition;
            this.originator = originator;
        }
        public void Fade()
        {
            GetNode<AnimationPlayer>("anim").Play("fade");
        }
        public void Attack(bool ignoreArmor = false, Dictionary<string, Dictionary<string, ushort>> attackTable = null)
        {
            if (attackTable == null)
            {
                attackTable = Stats.attackTable;
            }
            GD.Randomize();
            short damage = (short)Math.Round(GD.RandRange((double)originator.minDamage, (double)originator.maxDamage));
            ushort diceRoll = (ushort)(GD.Randi() % 100 + 1);
            string weaponTypeName = Enum.GetName(typeof(WorldTypes), weaponType);
            string swingTypeName = Enum.GetName(typeof(WorldTypes), swingType).ToLower();
            uint sndIdx = GD.Randi() % Globals.WEAPON_TYPE[weaponTypeName];
            bool PlaySound = false;
            string snd = $"{weaponTypeName.ToLower()}{sndIdx}";
            CombatText.TextType hitType;
            if (diceRoll <= attackTable["RANGED"]["HIT"])
            {
                hitType = CombatText.TextType.HIT;
            }
            else if (diceRoll <= attackTable["RANGED"]["CRITICAL"])
            {
                hitType = CombatText.TextType.CRITICAL;
                damage *= 2;
            }
            else if (diceRoll <= attackTable["RANGED"]["DODGE"])
            {
                hitType = CombatText.TextType.DODGE;
                damage = 0;
                snd = swingTypeName + GD.Randi() % Globals.WEAPON_TYPE[nameof(swingType)];
            }
            else
            {
                hitType = CombatText.TextType.MISS;
                damage = 0;
                snd = swingTypeName + GD.Randi() % Globals.WEAPON_TYPE[nameof(swingType)];
            }
            target.TakeDamage(damage, ignoreArmor, this, hitType);
            if (PlaySound)
            {
                Globals.PlaySound(snd, this, GetNode<AudioStreamPlayer2D>("snd"));
            }
        }
        public void Make()
        {
            string texturePath = "res://asset/img/missile-spell/{0}.res";
            string textureSize = "big";
            string raceName = originator.GetNode<Sprite>("img").GetTexture().GetPath().GetBaseDir().GetFile();
            switch (raceName)
            {
                case "gnoll":
                case "goblin":
                    GetNode<Sprite>("img").SetOffset(new Vector2(-5.5f, 0.0f));
                    textureSize = "small";
                    break;
            }
            if (spell == null)
            {
                texturePath = string.Format(texturePath, "arrow_{0}0");
            }
            else
            {
                string spellEffectPath = $"res://src/spell/effects/{spell.GetWorldName()}.tscn";
                if (new File().FileExists(spellEffectPath))
                {
                    PackedScene effectScene = (PackedScene)GD.Load(texturePath);
                    SpellEffect spellEffect = (SpellEffect)effectScene.Instance();
                    Connect(nameof(Hit), spellEffect, nameof(SpellEffect.OnHit));
                    if (spell.GetWorldType() != WorldTypes.SLOW)
                    {
                        spellEffect.Connect(nameof(SpellEffect.Unmake), this, nameof(Fade));
                    }
                    AddChild(spellEffect);
                    spellEffect.SetOwner(this);
                }
                switch (spell.GetWorldType())
                {
                    case WorldTypes.FIREBALL:
                    case WorldTypes.SHADOW_BOLT:
                    case WorldTypes.FROST_BOLT:
                    case WorldTypes.MIND_BLAST:
                    case WorldTypes.SLOW:
                    case WorldTypes.SIPHON_MANA:
                        GetNode<CollisionShape2D>("coll").SetShape((Shape2D)GD.Load("res://asset/img/missile-spell/spell_coll.res"));
                        switch (spell.GetWorldType())
                        {
                            case WorldTypes.MIND_BLAST:
                            case WorldTypes.SLOW:
                                instantSpawn = true;
                                break;
                            case WorldTypes.SIPHON_MANA:
                                instantSpawn = true;
                                reverse = true;
                                break;
                            case WorldTypes.FROST_BOLT:
                                rotate = true;
                                break;
                        }
                        break;
                    case WorldTypes.PIERCING_SHOT:
                        texturePath = string.Format(texturePath, "arrow_{0}1");
                        break;
                    case WorldTypes.CONCUSSIVE_SHOT:
                        texturePath = string.Format(texturePath, "arrow_{0}2");
                        break;
                    case WorldTypes.EXPLOSIVE_ARROW:
                        texturePath = string.Format(texturePath, "arrow_{0}3");
                        break;
                    default:
                        if (spell.GetWorldName().Contains("shot") ||
                            spell.GetWorldName().Contains("arrow") ||
                            spell.GetWorldType() == WorldTypes.VOLLEY)
                        {
                            texturePath = string.Format(texturePath, "arrow_{0}0");
                        }
                        break;
                }
            }
            if (texturePath.Contains("{0}") && texturePath.Contains("arrow"))
            {
                texturePath = string.Format(texturePath, textureSize);
                rotate = true;
                weaponType = WorldTypes.ARROW_HIT_ARMOR;
                swingType = WorldTypes.ARROW_PASS;
                GetNode<Node2D>("coll").SetPosition(new Vector2(-6.0f, 0.0f));
                GetNode<Sprite>("img").SetTexture((Texture)GD.Load(texturePath));
            }
        }
    }
}