using Godot;
namespace Game.Utils
{
    public class IconGenerator : Sprite
    {
        private const int LIMIT = 1790;
        private const int TILE_SIZE = 16;
        private Texture img;
        private Vector2 imgSize;

        public override void _Ready()
        {
            img = Texture;
            imgSize = img.GetSize();
            Generate();
        }
        private void Generate()
        {
            int count = 0;
            for (int c = 0; c < imgSize.y && count < LIMIT; c += TILE_SIZE)
            {
                for (int r = 0; r < imgSize.x && count < LIMIT; r += TILE_SIZE)
                {
                    AtlasTexture icon = new AtlasTexture();
                    icon.Atlas = img;
                    icon.Region = new Rect2(r, c, TILE_SIZE, TILE_SIZE);
                    ResourceSaver.Save($"res://asset/img/icon/raw/{count++}_icon.tres", icon);
                }
            }
            GD.Print("DONE");
        }
    }
}