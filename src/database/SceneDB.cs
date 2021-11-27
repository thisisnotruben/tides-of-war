using Godot;
namespace Game.Database
{
	public static class SceneDB
	{
		public static readonly PackedScene
			footStep = GD.Load<PackedScene>("res://src/character/doodads/FootStep.tscn"),
			combatText = GD.Load<PackedScene>("res://src/character/doodads/CombatText.tscn"),
			tomb = GD.Load<PackedScene>("res://src/character/doodads/Tomb.tscn"),
			treasureChest = GD.Load<PackedScene>("res://src/item/TreasureChest.tscn"),
			landMine = GD.Load<PackedScene>("res://src/landMine/LandMine.tscn"),
			moveCursor = GD.Load<PackedScene>("res://src/menu_ui/cursor/MoveCursorView.tscn"),
			missile = GD.Load<PackedScene>("res://src/missile/Missile.tscn"),
			missileSpell = GD.Load<PackedScene>("res://src/missile/MissileSpell.tscn"),
			missileSpelOrbital = GD.Load<PackedScene>("res://src/missile/MissileSpellOrbital.tscn"),
			spellAreaEffect = GD.Load<PackedScene>("res://src/spell/areaEffect/SpellAreaEffect.tscn"),
			stompAreaEffect = GD.Load<PackedScene>("res://src/spell/areaEffect/Stomp.tscn"),
			arcaneBoltAreaEffect = GD.Load<PackedScene>("res://src/spell/areaEffect/ArcaneBolt.tscn"),
			buffAnimScene = GD.Load<PackedScene>("res://src/character/doodads/BuffAnim.tscn"),
			loadEntryScene = GD.Load<PackedScene>("res://src/menu_ui/saveLoad/LoadEntryView.tscn"),
			npcScene = GD.Load<PackedScene>("res://src/character/npc/npc.tscn");

		public static readonly Resource
			ghostMaterial = GD.Load<ShaderMaterial>("res://asset/img/map/ghost.tres"),
			ghostShader = GD.Load<Shader>("res://asset/img/map/ghost.shader");
	}
}