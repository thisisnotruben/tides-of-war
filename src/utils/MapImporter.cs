using System;
using System.Collections.Generic;
using Godot;
namespace Game.Utils
{
    public class MapImporter : Node
    {
        private string scenePath = "res://src/map/zone_4.tscn";
        private readonly Vector2 CELL_SIZE = new Vector2(16.0f, 16.0f);
        private readonly Vector2 HALF_CELL_SIZE = new Vector2(8.0f, 8.0f);
        private TileMap tileMap;
        [Signal]
        public delegate void SendScenePath(string scenePath);
        public override void _Ready()
        {
            Connect(nameof(SendScenePath), GetNode(nameof(ColorRect)), "set_map_script", new Godot.Collections.Array() { scenePath });
            EmitSignal(nameof(SendScenePath));
            PackedScene packedScene = (PackedScene)GD.Load(scenePath);
            Godot.Error code = CheckMap(packedScene);
            GD.Print("Error code: " + code.ToString());
            if (code == Godot.Error.Ok)
            {
                GetNode<ColorRect>(nameof(ColorRect)).Color = new Color("#00ff00");
            }
        }
        private Godot.Error CheckMap(PackedScene mapToImport)
        {
            Node map = mapToImport.Instance();
            tileMap = map.GetNode<TileMap>("zed/z1");

            PackedScene dayTimeScene = (PackedScene)GD.Load("res://src/map/doodads/day_time.tscn");
            Node dayTime = dayTimeScene.Instance();
            map.AddChild(dayTime);
            dayTime.Owner = map;

            PackedScene veilScene = (PackedScene)GD.Load("res://src/map/doodads/veil_fog.tscn");
            Node2D veil = (Node2D)veilScene.Instance();
            map.AddChild(veil);
            veil.Owner = map;
            veil.Hide();

            foreach (Node2D node2D in map.GetNode("meta/gravesites").GetChildren())
            {
                node2D.GlobalPosition = GetCenterPos(node2D.GlobalPosition);
                node2D.AddToGroup("gravesite", true);
            }

            SetUnits(map);
            SetTargetDummys(map);
            SetLights(map);
            TreeUseMaterial(map);

            foreach (String nodePath in new String[] { "zed/characters", "zed/target_dummys", "meta/paths", "meta/lights" })
            {
                map.GetNode(nodePath).Owner = null;
            }

            map.GetNode<TileMap>("meta/coll_nav").Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            map.GetNode<TileMap>("zed/z1").CellYSort = true;
            map.GetNode<Node2D>("meta").Hide();
            foreach (Node node in map.GetNode("zed/z1").GetChildren())
            {
                if (node.Name.Contains("player"))
                {
                    map.GetNode("zed/z1").MoveChild(node, 0);
                    break;
                }
            }
            map.GetNode("meta").MoveChild(map.GetNode("meta/coll_nav"), 0);
            map.MoveChild(map.GetNode("day_time"), 0);
            map.MoveChild(map.GetNode("veil_fog"), 1);
            map.MoveChild(map.GetNode("meta"), 2);
            PackedScene packedScene = new PackedScene();
            packedScene.Pack(map);
            return ResourceSaver.Save(map.Filename, packedScene);
        }
        private void SetTargetDummys(Node map)
        {
            foreach (Node2D node in map.GetNode("zed/target_dummys").GetChildren())
            {
                PackedScene scene = (PackedScene)GD.Load("res://src/character/target_dummy/target_dummy.tscn");
                Node2D character = (Node2D)scene.Instance();
                character.Name = node.Name;
                map.GetNode("zed/z1").AddChild(character);
                character.Owner = map;
                character.GlobalPosition = GetCenterPos(node.GlobalPosition);
            }
        }
        private void SetUnits(Node map)
        {
            string scenePath = "res://src/character/{0}.tscn";
            foreach (Node2D node in map.GetNode("zed/characters").GetChildren())
            {
                PackedScene scene = (PackedScene)GD.Load(
                    String.Format(scenePath, (node.Name.Contains("player")) ? "player/player" : "npc/npc"));
                Node2D character = (Node2D)scene.Instance();
                character.Name = node.Name;
                map.GetNode("zed/z1").AddChild(character);
                character.Owner = map;
                character.GlobalPosition = GetCenterPos(node.GlobalPosition);
            }
        }
        private void SetLights(Node map)
        {
            string resourcePath = "res://src/misc/light/{0}.tscn";
            foreach (Node2D node in map.GetNode("meta/lights").GetChildren())
            {
                string parsedName = node.Name.Split("-")[1];
                PackedScene lightScene = (PackedScene)GD.Load(String.Format(resourcePath, parsedName));
                Node2D light = (Node2D)lightScene.Instance();
                Vector2 pos = node.GlobalPosition;
                if (parsedName.Equals("pit"))
                {
                    pos = tileMap.WorldToMap(node.GlobalPosition);
                    pos.y -= 1.0f;
                    pos = tileMap.MapToWorld(pos) + HALF_CELL_SIZE;
                }
                else
                {
                    // for all posts
                    pos.x += HALF_CELL_SIZE.x;
                }
                light.GlobalPosition = pos;
                map.GetNode("zed/z1").AddChild(light);
                light.Owner = map;
                light.Name = node.Name;
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
                    node2D.UseParentMaterial = true;
                }
                nodes.Add(node);
            }
            return nodes;
        }
        private Vector2 GetCenterPos(Vector2 worldPosition)
        {
            return tileMap.MapToWorld(tileMap.WorldToMap(worldPosition)) + HALF_CELL_SIZE;
        }
    }
}