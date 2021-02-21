using System.Collections.Generic;
using System;
using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Map
{
	public class Map : Node2D
	{
		private const float ASTAR_NORMAL_WEIGHT = 1.0f,
			ASTAR_OCCUPIED_WEIGHT = 50.0f,
			ASTAR_ITEM_WEIGHT = 25.0f;
		private static readonly Vector2 HALF_CELL_SIZE = new Vector2(8.0f, 8.0f);
		public static Map map;

		private readonly AStar2D aStar = new AStar2D();
		private TileMap collNav;
		private Vector2 mapSize;

		private Node2D ground, zed;
		private Particles2D veilFog;

		public Map() { map = this; }
		public override void _Ready()
		{
			collNav = GetNode<TileMap>("meta/coll_nav");
			ground = GetNode<Node2D>("ground");
			zed = GetNode<Node2D>("zed/z1");
			veilFog = GetNode<Particles2D>("VeilFog");

			mapSize = collNav.GetUsedRect().Size;
			MakeNav();
			SetVeilSize();
			SetCameraLimits(Player.player.camera);
		}
		public void AddGChild(Node node) { ground.AddChild(node); }
		public void AddZChild(Node node) { zed.AddChild(node); }
		public void SetVeil(bool on) { veilFog.Visible = veilFog.Emitting = on; }
		private void SetCameraLimits(Camera2D camera2D)
		{
			Rect2 mapBorders = collNav.GetUsedRect();
			Vector2 mapCellSize = collNav.CellSize;
			camera2D.LimitLeft = (int)(mapBorders.Position.x * mapCellSize.x);
			camera2D.LimitRight = (int)(mapBorders.End.x * mapCellSize.x);
			camera2D.LimitTop = (int)(mapBorders.Position.y * mapCellSize.y);
			// +68 for hud size
			camera2D.LimitBottom = (int)(mapBorders.End.y * mapCellSize.y) + 68;
		}
		private void SetVeilSize()
		{
			Vector2 veilSize = mapSize * HALF_CELL_SIZE;
			ParticlesMaterial veilMaterial = (ParticlesMaterial)veilFog.ProcessMaterial;
			veilMaterial.EmissionBoxExtents = new Vector3(veilSize.x, veilSize.y, 0.0f);
			veilFog.Amount = (int)((veilSize.x + veilSize.y) / 2.0f);
			veilFog.VisibilityRect = new Rect2(-veilSize, veilSize * 2.0f);
			veilFog.GlobalPosition = veilSize;
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
		public Queue<Vector2> getAPath(Vector2 worldStart, Vector2 worldEnd)
		{
			int worldStartPointIdx = GetPointIndex(collNav.WorldToMap(worldStart)),
				worldEndPointIdx = GetPointIndex(collNav.WorldToMap(worldEnd));

			if (aStar.HasPoint(worldStartPointIdx) && aStar.HasPoint(worldEndPointIdx))
			{
				Queue<Vector2> path = new Queue<Vector2>(aStar.GetPointPath(worldStartPointIdx, worldEndPointIdx));
				// first point is where worldStart is at
				path.Dequeue();
				return path;
			}
			return new Queue<Vector2>();
		}
		public Vector2 GetGridPosition(Vector2 worldPosition)
		{
			worldPosition = collNav.WorldToMap(worldPosition);
			int pointIndex = GetPointIndex(worldPosition);
			return aStar.HasPoint(pointIndex) ? aStar.GetPointPosition(pointIndex) : worldPosition;
		}
		public bool IsValidMove(Vector2 currentWorldPosition, Vector2 targetWorldPosition)
		{
			int pointIndex = GetPointIndex(collNav.WorldToMap(targetWorldPosition));
			return aStar.HasPoint(pointIndex)
				&& aStar.GetPointWeightScale(pointIndex) == ASTAR_NORMAL_WEIGHT
				&& !GetDirection(currentWorldPosition, targetWorldPosition).Equals(Vector2.Zero);
		}
		public Vector2 GetDirection(Vector2 currentWorldPosition, Vector2 targetWorldPosition)
		{
			currentWorldPosition = GetGridPosition(currentWorldPosition);
			targetWorldPosition = GetGridPosition(targetWorldPosition);
			Vector2 direction = Vector2.Zero;

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
		public Queue<Vector2> GetAttackSlot(Vector2 currentWorldPosition, Vector2 targetWorldPosition)
		{
			int localY, localX, pointLocalIndex;
			Vector2 pointLocal, randomizedSlot, targetCell = collNav.WorldToMap(targetWorldPosition);
			List<Vector2> pointList = new List<Vector2>();

			for (localY = 0; localY < 3; localY++)
			{
				for (localX = 0; localX < 3; localX++)
				{
					pointLocal = new Vector2(targetCell.x + localX - 1, targetCell.y + localY - 1);
					pointLocalIndex = GetPointIndex(pointLocal);
					if (aStar.HasPoint(pointLocalIndex) && aStar.GetPointWeightScale(pointLocalIndex) == ASTAR_NORMAL_WEIGHT)
					{
						pointList.Add(pointLocal);
					}
				}
			}
			GD.Randomize();
			randomizedSlot = collNav.MapToWorld(pointList[(int)GD.Randi() % pointList.Count]) + HALF_CELL_SIZE;
			return getAPath(currentWorldPosition, randomizedSlot);
		}
		public Vector2 SetGetPickableLoc(Vector2 worldPosition, bool setWeight)
		{
			aStar.SetPointWeightScale(
				GetPointIndex(collNav.WorldToMap(worldPosition)),
				setWeight ? ASTAR_ITEM_WEIGHT : ASTAR_NORMAL_WEIGHT
			);
			return GetGridPosition(worldPosition);
		}
		public void OccupyCell(Vector2 worldPosition, bool occupy)
		{
			int pointIndex = GetPointIndex(collNav.WorldToMap(worldPosition));
			if (aStar.HasPoint(pointIndex))
			{
				aStar.SetPointWeightScale(pointIndex, occupy ? ASTAR_OCCUPIED_WEIGHT : ASTAR_NORMAL_WEIGHT);
			}
		}
	}
}