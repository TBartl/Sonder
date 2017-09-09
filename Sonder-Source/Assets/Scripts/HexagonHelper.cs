using UnityEngine;
using System.Collections;

[System.Serializable]
public enum HexType {
	None, Earth, StoneFoundation, StoneConnectTemp, Stairs0, Stairs90, Stairs180, Stairs270
}

public enum HexDraw {
	both, bottom, middle, top, cityHut0, cityHut1, cityHut2, cityHut3, stairs1, stairs2, stairs3
}

[System.Serializable]
public enum HexDir {
	DownRight, Down, DownLeft, UpLeft, Up, UpRight
}

[System.Serializable]
public class HexHelp {
	public static float radius = 2*2;
	public static float width = 4*2;
	public static float height = 3.46410161514f*2;
	public static float widthCutoff = 3*2;
	
	public static Vector2 MoveInDirection(Vector2 startingPoint, HexDir dir) {
		switch (dir) {
		case HexDir.Up:
			return startingPoint + new Vector2(0,1);
			
		case HexDir.UpRight:
			return startingPoint + new Vector2(1,0);
			
		case HexDir.DownRight:
			return startingPoint + new Vector2(1,-1);
			
		case HexDir.Down:
			return startingPoint + new Vector2(0,-1);
			
		case HexDir.DownLeft:
			return startingPoint + new Vector2(-1,0);
			
		case HexDir.UpLeft:
			return startingPoint + new Vector2(-1,1);

		default:
			Debug.Log("Wat");
			return startingPoint;

		}
	}

	public static Vector2 GetCenter(int rings) {
		return Vector2.one * (rings - 1);
	}
	
	
	public static Vector2 GetRelativePosition(Vector2 tileIndexes, int rings) {
		Vector2 offset = tileIndexes - GetCenter (rings);
		Vector2 temp = Vector2.zero;
		temp.x = offset.x * widthCutoff;
		temp.y = height * (offset.y + offset.x / 2);

		return temp;
	}


	public static bool CheckBelow(int x, int y, int layer, HexType[,,] tiles) {
		bool bottomLayer = (layer == 0);
		if (bottomLayer) {
			return false;
		} else {
			return ( tiles [layer-1, y, x] != HexType.None );
		}
	}
	public static bool CheckAbove(int x, int y, int layer, HexType[,,] tiles) {
		bool topLayer = (layer == tiles.GetLength(0)-1);
		if (topLayer) {
			return false;
		} else {
			return ( tiles [layer+1, y, x] != HexType.None );
		}
	}

	public static bool CheckAboveMinusHuts(int x, int y, int layer, HexType[,,] tiles) {
		bool topLayer = (layer == tiles.GetLength(0)-1);
		if (topLayer) {
			return false;
		} else {
			return ( tiles [layer+1, y, x] != HexType.None && tiles [layer+1, y, x] != HexType.StoneFoundation );
		}
	}
	
	public static bool CheckDirRaw(int x, int y, int layer, HexType[,,] tiles) {
		bool inBounds = (x >= 0) && (y >= 0) && (x < tiles.GetLength (2)) && (y < tiles.GetLength (1));
		if (inBounds) {
			return tiles[layer, y, x] != HexType.None;
		} else {
			return false;
		}
	}

	public static bool CheckForStairs(int x, int y, int layer, HexType[,,] tiles) {
		bool inBounds = (x >= 0) && (y >= 0) && (x < tiles.GetLength (2)) && (y < tiles.GetLength (1));
		if (inBounds) {
			return tiles[layer, y, x] == HexType.Stairs0 || 
					tiles[layer, y, x] == HexType.Stairs90 || 
					tiles[layer, y, x] == HexType.Stairs180 || 
					tiles[layer, y, x] == HexType.Stairs270;
		} else {
			return false;
		}
	}

}


