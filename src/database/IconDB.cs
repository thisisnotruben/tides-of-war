using Godot;
namespace Game.Database
{
	public static class IconDB
	{
		private static readonly Texture img = (Texture)GD.Load("res://asset/img/icon/icons.png");
		private const int atlasCellWidth = 16;
		private static readonly Vector2 iconSize = new Vector2(16.0f, 16.0f);

		public static AtlasTexture GetIcon(int iconID)
		{
			AtlasTexture atlasTexture = new AtlasTexture();
			atlasTexture.Atlas = img;
			Vector2 pos = new Vector2(iconID % atlasCellWidth * iconSize.x, iconID / atlasCellWidth * iconSize.y);
			atlasTexture.Region = new Rect2(pos, iconSize);
			return atlasTexture;
		}
	}
}