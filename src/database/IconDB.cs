using Godot;
namespace Game.Database
{
    public static class IconDB
    {
        private static readonly Texture img = (Texture) GD.Load("res://asset/img/icon/icons.png");
        private static int atlasWidth = 16;
        private static Vector2 iconSize = new Vector2(16, 16);

        public static AtlasTexture GetIcon(int iconID)
        {
            AtlasTexture atlasTexture = new AtlasTexture();
            atlasTexture.Atlas = img;
            Vector2 pos = new Vector2(iconID % atlasWidth * iconSize.x, iconID / atlasWidth * iconSize.y);
            atlasTexture.Region = new Rect2(pos, iconSize);
            return atlasTexture;
        }
    }
}