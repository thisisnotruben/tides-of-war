using System.Collections.Generic;
using Game.Actor;
using Godot;
namespace Game.Ui
{
	public class MiniMapController : Node2D
	{
		private readonly Vector2 drawSize = new Vector2(14.0f, 14.0f),
			offset = new Vector2(7.0f, 0.0f);
		private List<Vector2> path = new List<Vector2>();
		private Vector2 mapRatio = new Vector2();
		private Player player;

		public override void _Ready()
		{
			SetProcess(false);
			// player = ((InGameMenu)Owner).player;
			// return;
			// string mapName = Map.Map.map.Name;
			// string miniMapPath = $"res://asset/img/map/{mapName}.png";
			// if (new Directory().FileExists(miniMapPath))
			// {
			// 	GetNode<Sprite>("map").Texture = (Texture)GD.Load(miniMapPath);
			// }
		}
		public override void _Process(float delta)
		{
			if (player.moving)
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
			Vector2 playerPos = GetNode<Node2D>("player_pos").GlobalPosition;
			foreach (Character character in GetTree().GetNodesInGroup("character"))
			{
				Rect2 rect2 = new Rect2(playerPos - ScaleToMapRatio(player.pos - character.pos) - offset, drawSize);
				Color rectColor = new Color("#ffffff");
				// switch (character.worldType)
				// {
				//     case Character.WorldTypes.PLAYER:
				//         break;
				//     case Character.WorldTypes.QUEST_GIVER:
				//         rectColor = new Color("#ffff00");
				//         break;
				//     case Character.WorldTypes.TRAINER:
				//         rectColor = new Color("#00ffff");
				//         break;
				//     case Character.WorldTypes.MERCHANT:
				//         rectColor = new Color("#00ff00");
				//         break;
				//     case Character.WorldTypes.HEALER:
				//         break;
				// }
				DrawRect(rect2, rectColor);
				if (player.dead && path.Count > 0)
				{
					for (int i = 0; i < path.Count - 1; i++)
					{
						DrawLine(playerPos - ScaleToMapRatio(player.GlobalPosition - path[i]),
							playerPos - ScaleToMapRatio(player.GlobalPosition - path[i + 1]),
							new Color("#ffffff"), 4.0f);
					}
					DrawRect(new Rect2(playerPos - ScaleToMapRatio(player.GlobalPosition - path[path.Count - 1]) - offset,
						drawSize), new Color("#94d0d5"));
				}
			}
			DrawRect(new Rect2(playerPos - offset, drawSize), new Color("#ffffff"));
		}
		public void _OnMiniMapDraw()
		{
			if (mapRatio.Equals(new Vector2()))
			{
				TileMap tileMap = Map.Map.map.GetNode<TileMap>("ground/g1");
				Sprite map = GetNode<Sprite>("map");
				mapRatio = tileMap.GetUsedRect().Size * tileMap.CellSize / (map.Texture.GetSize() * map.Scale);
				mapRatio = new Vector2(1.0f / mapRatio.x, 1.0f / mapRatio.y);
			}
			// if (player.dead && !player.gravePos.Equals(new Vector2()) && path.Count == 0)
			// {
			//     path = Map.Map.map.getAPath(player.GlobalPosition, player.gravePos);
			// }
			GetNode<Node2D>("map").Position = GetNode<Node2D>("player_pos").GlobalPosition - ScaleToMapRatio(player.pos);
			SetProcess(true);
		}
		public void _OnMiniMapHide()
		{
			path.Clear();
			SetProcess(false);
		}
		private Vector2 ScaleToMapRatio(Vector2 input) { return new Vector2(input.x * mapRatio.x, input.y * mapRatio.y); }
	}
}