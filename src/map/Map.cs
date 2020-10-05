using System.Collections.Generic;
using System;
using Game.Database;
using Godot;
namespace Game.Map
{
	public class Map : Node2D
	{
		private TileMap collNav;
		private readonly AStar2D aStar = new AStar2D();
		private static readonly Vector2 HALF_CELL_SIZE = new Vector2(8.0f, 8.0f);
		private const float ASTAR_OCCUPIED_WEIGHT = 50.0f,
			ASTAR_ITEM_WEIGHT = 25.0f,
			ASTAR_NORMAL_WEIGHT = 1.0f;
		private Vector2 mapSize;
		public static Map map;

		public Map() { map = this; }
		public override void _Ready()
		{
			collNav = GetNode<TileMap>("meta/coll_nav");
			mapSize = collNav.GetUsedRect().Size;
			MakeNav();
			SetVeilSize();
			SetPlayerCameraLimits();
		}
		public void AddZChild(Node node) { GetNode("zed/z1").AddChild(node); }
		public void SetVeil(bool on)
		{
			Particles2D veil = GetNode<Particles2D>("VeilFog");
			veil.Emitting = on;
			if (on)
			{
				Material = (ShaderMaterial)GD.Load("res://asset/img/map/veil.tres");
				veil.Show();
			}
			else
			{
				Material = null;
				veil.Hide();
			}
		}
		private void SetPlayerCameraLimits()
		{
			Rect2 mapBorders = collNav.GetUsedRect();
			Vector2 mapCellSize = collNav.CellSize;
			Camera2D playerCamera = Actor.Player.player.GetNode<Camera2D>("img/camera");
			playerCamera.LimitLeft = (int)(mapBorders.Position.x * mapCellSize.x);
			playerCamera.LimitRight = (int)(mapBorders.End.x * mapCellSize.x);
			playerCamera.LimitTop = (int)(mapBorders.Position.y * mapCellSize.y);
			playerCamera.LimitBottom = (int)(mapBorders.End.y * mapCellSize.y);
		}
		private void SetVeilSize()
		{
			Vector2 veilSize = mapSize * HALF_CELL_SIZE;
			Particles2D veil = GetNode<Particles2D>("VeilFog");
			ParticlesMaterial veilMaterial = (ParticlesMaterial)veil.ProcessMaterial;
			veilMaterial.EmissionBoxExtents = new Vector3(veilSize.x, veilSize.y, 0.0f);
			veil.Amount = (int)((veilSize.x + veilSize.y) / 2.0f);
			veil.VisibilityRect = new Rect2(-veilSize, veilSize * 2.0f);
			veil.GlobalPosition = veilSize;
		}
		private void MakeNav()
		{
			aStar.Clear();

			int x, y, tileId, fromIdx, toX, toY, toIdx, toTileId;
			Vector2 from, to, direction;
			CollNavDB.collNavTile fromTile;
			CollNavDB.Ordinal ordinal;

			// for each tile in map
			for (y = 0; y < (int)mapSize.y; y++)
			{
				for (x = 0; x < (int)mapSize.x; x++)
				{
					tileId = collNav.GetCell(x, y);
					if (!Enum.IsDefined(typeof(CollNavDB.collNavTile), tileId) || tileId == (int)CollNavDB.collNavTile.BLOCK)
					{
						continue;
					}

					fromTile = (CollNavDB.collNavTile)tileId;
					from = new Vector2(x, y);
					fromIdx = GetPointIndex(from);

					// Add node
					aStar.AddPoint(fromIdx, collNav.MapToWorld(from) + HALF_CELL_SIZE, ASTAR_NORMAL_WEIGHT);

					// add edges from neighbors
					for (toY = -1; toY <= 1; toY++)
					{
						for (toX = -1; toX <= 1; toX++)
						{
							to = new Vector2(from.x + toX, from.y + toY);
							toIdx = GetPointIndex(to);

							if (fromIdx == toIdx || IsOutsideMapBounds(to))
							{
								continue;
							}

							toTileId = collNav.GetCellv(to);
							if (!Enum.IsDefined(typeof(CollNavDB.collNavTile), toTileId) || toTileId == (int)CollNavDB.collNavTile.BLOCK)
							{
								continue;
							}

							// get ordinal from direction
							direction = to - from;
							if (direction.Equals(Vector2.Up + Vector2.Left))
							{
								ordinal = CollNavDB.Ordinal.NW;
							}
							else if (direction.Equals(Vector2.Up))
							{
								ordinal = CollNavDB.Ordinal.N;
							}
							else if (direction.Equals(Vector2.Up + Vector2.Right))
							{
								ordinal = CollNavDB.Ordinal.NE;
							}
							else if (direction.Equals(Vector2.Left))
							{
								ordinal = CollNavDB.Ordinal.W;
							}
							else if (direction.Equals(Vector2.Right))
							{
								ordinal = CollNavDB.Ordinal.E;
							}
							else if (direction.Equals(Vector2.Down + Vector2.Left))
							{
								ordinal = CollNavDB.Ordinal.SW;
							}
							else if (direction.Equals(Vector2.Down))
							{
								ordinal = CollNavDB.Ordinal.S;
							}
							else if (direction.Equals(Vector2.Down + Vector2.Right))
							{
								ordinal = CollNavDB.Ordinal.SE;
							}
							else
							{
								continue;
							}

							// connect by the rules in 'collNav.json'
							if (CollNavDB.CanConnect(fromTile, (CollNavDB.collNavTile)toTileId, ordinal))
							{
								aStar.AddPoint(toIdx, collNav.MapToWorld(to) + HALF_CELL_SIZE, ASTAR_NORMAL_WEIGHT);
								aStar.ConnectPoints(fromIdx, toIdx);
							}
						}
					}
				}
			}
		}
		private bool IsOutsideMapBounds(Vector2 point)
		{
			return point.x < 0.0f || point.y < 0.0f || point.x >= mapSize.x || point.y >= mapSize.y;
		}
		private int GetPointIndex(Vector2 point)
		{
			// Cantor pairing function
			int x = (int)point.x, y = (int)point.y;
			return ((x + y) * (x + y + 1)) / 2 + y;
		}
		private void SetPointWeight(Vector2 worldPosition, float weight)
		{
			aStar.SetPointWeightScale(GetPointIndex(collNav.WorldToMap(worldPosition)), weight);
		}
		public List<Vector2> getAPath(Vector2 worldStart, Vector2 worldEnd)
		{
			List<Vector2> worldPath = new List<Vector2>();
			int worldStartPointIdx = GetPointIndex(collNav.WorldToMap(worldStart)),
				worldEndPointIdx = GetPointIndex(collNav.WorldToMap(worldEnd));

			if (aStar.HasPoint(worldStartPointIdx) && aStar.HasPoint(worldEndPointIdx))
			{
				worldPath.AddRange(aStar.GetPointPath(worldStartPointIdx, worldEndPointIdx));
			}
			return worldPath;
		}
		public Vector2 GetGridPosition(Vector2 worldPosition)
		{
			return collNav.MapToWorld(collNav.WorldToMap(worldPosition)) + HALF_CELL_SIZE;
		}
		public Vector2 RequestMove(Vector2 currentWorldPosition, Vector2 direction)
		{
			Vector2 cellStart = collNav.WorldToMap(currentWorldPosition);
			Vector2 cellTarget = cellStart + direction;
			int targetPointIndex = GetPointIndex(cellTarget);
			if (aStar.HasPoint(targetPointIndex) && aStar.GetPointWeightScale(targetPointIndex) == ASTAR_NORMAL_WEIGHT)
			{
				int currentPointIndex = GetPointIndex(cellStart);
				if (aStar.GetPointWeightScale(currentPointIndex) != ASTAR_ITEM_WEIGHT)
				{
					aStar.SetPointWeightScale(currentPointIndex, ASTAR_NORMAL_WEIGHT);
				}
				// aStar.SetPointWeightScale(targetPointIndex, ASTAR_OCCUPIED_WEIGHT);
				return collNav.MapToWorld(cellTarget) + HALF_CELL_SIZE;
			}
			return new Vector2();
		}
		public bool IsValidMove(Vector2 worldPosition)
		{
			int pointIndex = GetPointIndex(collNav.WorldToMap(worldPosition));
			return aStar.HasPoint(pointIndex) && aStar.GetPointWeightScale(pointIndex) == ASTAR_NORMAL_WEIGHT;
		}
		public void ResetPath(List<Vector2> pointList)
		{
			foreach (Vector2 point in pointList)
			{
				SetPointWeight(point, ASTAR_NORMAL_WEIGHT);
			}
		}
		public Vector2 GetDirection(Vector2 currentWorldPosition, Vector2 targetWorldPosition)
		{
			currentWorldPosition = GetGridPosition(currentWorldPosition);
			targetWorldPosition = GetGridPosition(targetWorldPosition);
			Vector2 direction = new Vector2();
			if (currentWorldPosition.x > targetWorldPosition.x)
			{
				direction.x = -1;
			}
			else if (currentWorldPosition.x < targetWorldPosition.x)
			{
				direction.x = 1;
			}
			if (currentWorldPosition.y > targetWorldPosition.y)
			{
				direction.y = -1;
			}
			else if (currentWorldPosition.y < targetWorldPosition.y)
			{
				direction.y = 1;
			}
			return direction;
		}

		public List<Vector2> GetAttackSlot(Vector2 currentWorldPosition, Vector2 targetWorldPosition)
		{
			Vector2 targetCell = collNav.WorldToMap(targetWorldPosition);
			List<Vector2> pointList = new List<Vector2>();
			for (int localY = 0; localY < 3; localY++)
			{
				for (int localX = 0; localX < 3; localX++)
				{
					Vector2 pointLocal = new Vector2(targetCell.x + localX - 1, targetCell.y + localY - 1);
					int pointLocalIndex = GetPointIndex(pointLocal);
					if (aStar.HasPoint(pointLocalIndex) && aStar.GetPointWeightScale(pointLocalIndex) != ASTAR_OCCUPIED_WEIGHT)
					{
						pointList.Add(pointLocal);
					}
				}
			}
			GD.Randomize();
			Vector2 randomizedSlot = collNav.MapToWorld(pointList[(int)GD.Randi() % pointList.Count]) + HALF_CELL_SIZE;
			return getAPath(currentWorldPosition, randomizedSlot);
		}
		public Vector2 SetGetPickableLoc(Vector2 worldPosition, bool setWeight)
		{
			SetPointWeight(worldPosition, (setWeight) ? ASTAR_ITEM_WEIGHT : ASTAR_NORMAL_WEIGHT);
			return GetGridPosition(worldPosition);
		}
		private void OccupyOriginCell(Vector2 worldPosition)
		{
			// Used in SetUnits method; used for placing units in they're origin cell

			// TODO
			// Vector2 point = collNav.WorldToMap(worldPosition);
			// if (!mapObstacles.Contains(point) && !IsOutsideMapBounds(point))
			// {
			//     int pointIndex = GetPointIndex(point);
			//     aStar.SetPointWeightScale(pointIndex, ASTAR_OCCUPIED_WEIGHT);
			// }
			// else
			// {
			//     GD.Print($"Object incorrectly placed at\ngrid position: {point}\nglobal position: {collNav.MapToWorld(point)}\n");
			// }
		}
	}
}