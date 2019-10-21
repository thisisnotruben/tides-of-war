using Godot;
using System.Collections.Generic;
using Game.Actor;

namespace Game.Ui
{
    public class MiniMap : Node2D
    {
        private readonly Vector2 drawSize = new Vector2(14.0f, 14.0f);
        private readonly Vector2 offset = new Vector2(7.0f, 0.0f);
        private List<Vector2> path = new List<Vector2>();
        private Vector2 mapRatio = new Vector2();
        private Player player;

        public override void _Ready()
        {
            player = Globals.player;
            SetProcess(false);
            return;
            string mapName = Globals.GetMap().GetName();
            string miniMapPath = string.Format("res://asset/img/map/{0}.png", mapName);
            if (new Directory().FileExists(miniMapPath))
            {
                GetNode<Sprite>("map").SetTexture((Texture)GD.Load(miniMapPath));
            }
            else
            {
                GD.Print("No mini-map found for map: " + mapName);
            }
        }
        public override void _Process(float delta)
        {
            if (player.GetState() == Character.States.MOVING)
            {
                Hide();
            }
            else
            {
                Update();
            }
        }
        public override void _Draw()
        {
            Vector2 playerPos = GetNode<Node2D>("player_pos").GetGlobalPosition();
            foreach (Character character in GetTree().GetNodesInGroup("npc"))
            {
                Rect2 rect2 = new Rect2(playerPos - ScaleToMapRatio(player.GetCenterPos() - character.GetCenterPos()) - offset, drawSize);
                Color rectColor = new Color("#ffffff");
                switch (character.GetWorldType())
                {
                    case Character.WorldTypes.PLAYER:
                        rectColor = new Color("#ff0000");
                        break;
                    case Character.WorldTypes.QUEST_GIVER:
                        rectColor = new Color("#ffff00");
                        break;
                    case Character.WorldTypes.TRAINER:
                        rectColor = new Color("#00ffff");
                        break;
                    case Character.WorldTypes.MERCHANT:
                        rectColor = new Color("#00ff00");
                        break;
                }
                DrawRect(rect2, rectColor);
                if (player.IsDead() && path.Count > 0)
                {
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        DrawLine(playerPos - ScaleToMapRatio(player.GetGlobalPosition() - path[i]),
                            playerPos - ScaleToMapRatio(player.GetGlobalPosition() - path[i + 1]),
                            new Color("#ffffff"), 4.0f);
                    }
                    DrawRect(new Rect2(playerPos - ScaleToMapRatio(player.GetGlobalPosition() - path[path.Count - 1]) - offset,
                        drawSize), new Color("#94d0d5"));
                }
            }
            DrawRect(new Rect2(playerPos - offset, drawSize), new Color("#ffffff"));
        }
        public void _OnMiniMapDraw()
        {
            if (mapRatio.Equals(new Vector2()))
            {
                TileMap tileMap = Globals.GetMap().GetNode<TileMap>("ground/g1");
                Sprite map = GetNode<Sprite>("map");
                mapRatio = tileMap.GetUsedRect().Size * tileMap.GetCellSize() / (map.GetTexture().GetSize() * map.GetScale());
                mapRatio = new Vector2(1.0f / mapRatio.x, 1.0f / mapRatio.y);
            }
            if (player.IsDead() && !player.GetGravePos().Equals(new Vector2()) && path.Count == 0)
            {
                path = Globals.GetMap().getAPath(player.GetGlobalPosition(), player.GetGravePos());
            }
            GetNode<Node2D>("map").SetPosition(GetNode<Node2D>("player_pos").GetGlobalPosition() - ScaleToMapRatio(player.GetCenterPos()));
            SetProcess(true);
        }
        public void _OnMiniMapHide()
        {
            path.Clear();
            SetProcess(false);
        }
        private Vector2 ScaleToMapRatio(Vector2 input)
        {
            return new Vector2(input.x * mapRatio.x, input.y * mapRatio.y);
        }
    }
}
