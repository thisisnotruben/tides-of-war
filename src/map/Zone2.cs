using Godot;
using Game.Database;
namespace Game.Map
{
	public class Zone2 : Map
	{
		public override void _Ready()
		{
			base._Ready();
			SetTilesetShader(true);
			Connect("tree_exiting", this, nameof(SetTilesetShader),
				new Godot.Collections.Array() { false });
		}
		public void SetTilesetShader(bool set)
		{
			((ShaderMaterial)SceneDB.ghostMaterial).Shader =
				set
				? (Shader)SceneDB.ghostShader
				: null;
		}
	}
}