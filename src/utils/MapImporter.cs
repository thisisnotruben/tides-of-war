using Godot;
using System;
using System.Collections.Generic;
namespace Game.Utils
{
    public class MapImporter : Node
    {
        private string scenePath = "res://src/map/zone_4.tscn";
        private int count = 0;
        private readonly Vector2 CELL_SIZE = new Vector2(16.0f, 16.0f);
        private readonly Vector2 HALF_CELL_SIZE = new Vector2(8.0f, 8.0f);
        private TileMap tileMap;
        [Signal]
        public delegate void SendScenePath(string scenePath);
        public override void _Ready()
        {
            Connect(nameof(SendScenePath), GetNode(nameof(ColorRect)), "set_map_script", new Godot.Collections.Array() {scenePath});
            EmitSignal(nameof(SendScenePath));
            PackedScene packedScene = (PackedScene)GD.Load(scenePath);
            Godot.Error code = CheckMap(packedScene);
            GD.Print("Error code: " + code.ToString());
            if (code == Godot.Error.Ok)
            {
                GetNode<ColorRect>(nameof(ColorRect)).SetFrameColor(new Color("#00ff00"));
            }
        }
        private Godot.Error CheckMap(PackedScene mapToImport)
        {
            Node map = mapToImport.Instance();
            tileMap = map.GetNode<TileMap>("zed/z1");
 
            PackedScene dayTimeScene = (PackedScene)GD.Load("res://src/map/doodads/DayTime.tscn");
            Node dayTime = dayTimeScene.Instance();
            map.AddChild(dayTime);
            dayTime.SetOwner(map);

            PackedScene veilScene = (PackedScene)GD.Load("res://src/map/doodads/veil_fog.tscn");
            Node2D veil = (Node2D)veilScene.Instance();
            map.AddChild(veil);
            veil.SetOwner(map);
            veil.Hide();

            foreach (Node2D node2D in map.GetNode("meta/gravesites").GetChildren())
            {
                node2D.SetGlobalPosition(GetCenterPos(node2D.GetGlobalPosition()));
                node2D.AddToGroup("gravesite", true);
            }

            SetUnits(map);
            SetTargetDummys(map);
            SetLights(map);
            TreeUseMaterial(map);

            foreach (String nodePath in new String[] {"zed/characters", "zed/target_dummys", "meta/paths", "meta/lights"})
            {
             map.GetNode(nodePath).SetOwner(null);
            }

            map.GetNode<TileMap>("meta/coll_nav").SetModulate(new Color(1.0f, 1.0f, 1.0f, 0.5f));
            map.GetNode<TileMap>("zed/z1").SetYSortMode(true);
            map.GetNode<Node2D>("meta").Hide();
            map.GetNode("zed/z1").MoveChild(map.GetNode("zed/z1/player"), 0);
            map.GetNode("meta").MoveChild(map.GetNode("meta/coll_nav"), 0);
            map.MoveChild(map.GetNode("day_time"), 0);
            map.MoveChild(map.GetNode("veil_fog"), 1);
            map.MoveChild(map.GetNode("meta"), 2);
            PackedScene packedScene = new PackedScene();
            packedScene.Pack(map);
            return ResourceSaver.Save(map.GetFilename(), packedScene);
        }
        private void SetTargetDummys(Node map)
        {
            foreach (Node2D node in map.GetNode("zed/target_dummys").GetChildren())
            {
                PackedScene scene = (PackedScene)GD.Load("res://src/character/target_dummy/target_dummy.tscn");
                Node2D character = (Node2D)scene.Instance();
                character.SetName($"{GetParsedName(node.GetName())}-{count++}");
                map.GetNode("zed/z1").AddChild(character);
                character.SetOwner(map);
                character.SetGlobalPosition(GetCenterPos(node.GetGlobalPosition()));
            }
        }
        private void SetUnits(Node map)
        {
            string scenePath = "res://src/character/{0}.tscn";
            foreach (Node2D node in map.GetNode("zed/characters").GetChildren())
            {
                PackedScene scene = (PackedScene)GD.Load(
                    String.Format(scenePath, (node.GetName().Contains("player")) ? "player/Player" : "npc/Npc"));
                Node2D character = (Node2D)scene.Instance();
                character.SetName(node.GetName());
                map.GetNode("zed/z1").AddChild(character);
                character.SetOwner(map);
                character.SetGlobalPosition(GetCenterPos(node.GetGlobalPosition()));
            }
        }
        private void SetLights(Node map)
        {
            string resourcePath = "res://src/misc/light/{0}.tscn";
            foreach (Node2D node in map.GetNode("meta/lights").GetChildren())
            {
                string parsedName = GetParsedName(node.GetName());
                PackedScene lightScene = (PackedScene)GD.Load(String.Format(resourcePath, parsedName));
                Node2D light = (Node2D)lightScene.Instance();
                Vector2 pos = node.GetGlobalPosition();
                if (parsedName.Contains("pit"))
                {
                    pos = tileMap.WorldToMap(node.GetGlobalPosition());
                    pos.y -= 1.0f;
                    pos = tileMap.MapToWorld(pos) + HALF_CELL_SIZE;
                }
                else
                {
                    // for all posts
                    pos.x += HALF_CELL_SIZE.x;
                }
                light.SetGlobalPosition(pos);
                map.GetNode("zed/z1").AddChild(light);
                light.SetOwner(map);
                light.SetName($"{parsedName}-{count++}");
            }
        }
        private List<Node> TreeUseMaterial(Node root)
        {
            List<Node> nodes = new List<Node>();
            foreach (Node node in root.GetChildren())
            {
                Node2D node2D = node as Node2D;
                if (node2D != null)
                {
                    node2D.SetUseParentMaterial(true);
                }
                nodes.Add(node);
            }
            return nodes;
        }
        private Vector2 GetCenterPos(Vector2 worldPosition)
        {
            return tileMap.MapToWorld(tileMap.WorldToMap(worldPosition)) + HALF_CELL_SIZE;
        }
        private String GetParsedName(String name)
        {
            string parsedName = "";
            foreach (char letter in name)
            {
                if (!Char.IsDigit(letter))
                {
                    parsedName += letter;
                }
            }
            return parsedName;
        }
    }
}