using UnityEngine;
using System.Collections;

[System.Serializable]
public class CityLayer {
	
	public float baseHutGenChance;
	public float connectionChance;
	public float stairChance;
	//public float fillChance; TODO Implement, set to 0 for layer 3
	public bool connectionRequiresLower;
	public bool singleHutsAllowed;
	public float openDoorChance;
	string[] doorFrameNames = new string[] {"TR", "T", "TL", "DL", "D", "DR"};
	

	public void GenerateHuts(int layerPos, HexType[,,] tiles) {
		for (int yIndex = 0; yIndex < tiles.GetLength(1); yIndex += 2) {
			for (int xIndex = 0; xIndex < tiles.GetLength(2); xIndex += 2) {
				tiles[layerPos, yIndex, xIndex] = HexType.None;
				if (Random.value <= baseHutGenChance && HexHelp.CheckBelow(xIndex,yIndex,layerPos,tiles))
					tiles[layerPos, yIndex, xIndex] = HexType.StoneFoundation;
			}
		}
	}

	public void GenerateConnections(int layerPos, HexType[,,] tiles) { 
		for (int yIndex = 0; yIndex < tiles.GetLength(1); yIndex += 1) {
			for (int xIndex = (yIndex+1)%2; xIndex < tiles.GetLength(2); xIndex += 2) {
				tiles[layerPos, yIndex, xIndex] = HexType.None;
				bool canConnect = SurroundedHorzOrVert(xIndex, yIndex, layerPos, tiles);
				bool hasBelow = HexHelp.CheckBelow(xIndex, yIndex, layerPos, tiles);
				if (canConnect && (hasBelow || !connectionRequiresLower) && Random.value <= connectionChance) {
					if (connectionRequiresLower) 
						tiles[layerPos, yIndex, xIndex] = HexType.StoneFoundation;
					else 
						tiles[layerPos, yIndex, xIndex] = HexType.StoneConnectTemp;
				}					
			}
		}
	}

	public void GenerateStairs(int layerPos, HexType[,,] tiles) {
		if (layerPos%2 == 0){
			for (int xIndex = 0; xIndex < tiles.GetLength(2); xIndex += 2) {
				for (int yIndex = 0; yIndex < tiles.GetLength(1); yIndex += 2) {
					AddSingleStair(xIndex, yIndex, layerPos, tiles);
				}	
			}
		}
		else {
			for (int xIndex = 0; xIndex < tiles.GetLength(2); xIndex += 1){
				for (int yIndex = 1 - xIndex%2; yIndex < tiles.GetLength(1); yIndex += 2) {
					AddSingleStair(xIndex, yIndex, layerPos, tiles);
				}
			}
		}
	}

	void AddSingleStair(int xIndex, int yIndex, int layerPos, HexType[,,] tiles)
	{
		if (tiles[layerPos, yIndex, xIndex] != HexType.None || layerPos%2 == 1)
		{
			bool left = HexHelp.CheckDirRaw (xIndex - 1, yIndex, layerPos, tiles);
			bool right = HexHelp.CheckDirRaw (xIndex + 1, yIndex, layerPos, tiles);
			bool top = HexHelp.CheckDirRaw (xIndex, yIndex + 1, layerPos, tiles);
			bool bottom = HexHelp.CheckDirRaw (xIndex, yIndex - 1, layerPos, tiles);
			int adjacent = 0;
			if (left) adjacent++;
			if (right) adjacent ++;
			if (bottom) adjacent ++;
			if (top) adjacent ++;
			
			bool hasBelow = HexHelp.CheckBelow(xIndex,yIndex,layerPos,tiles);
			bool hasAbove = HexHelp.CheckAbove(xIndex,yIndex,layerPos,tiles);

			if (hasBelow && !hasAbove  && adjacent == 1 && Random.value <= stairChance)
			{
				//
				if (top && HexHelp.CheckDirRaw(xIndex, yIndex - 1, layerPos - 1, tiles))
					tiles[layerPos, yIndex, xIndex] = HexType.Stairs0;

				else if (left && HexHelp.CheckDirRaw(xIndex + 1,yIndex,layerPos-1, tiles))
					tiles[layerPos, yIndex, xIndex] = HexType.Stairs90;

				else if (bottom && HexHelp.CheckDirRaw(xIndex, yIndex + 1, layerPos-1, tiles))
					tiles[layerPos, yIndex, xIndex] = HexType.Stairs180;

				else if (right && HexHelp.CheckDirRaw(xIndex - 1, yIndex, layerPos-1, tiles))
					tiles[layerPos, yIndex, xIndex] = HexType.Stairs270;
			}
		}
	}


	public void RemoveSingleHuts(int layerPos, HexType[,,] tiles) { 
		if (!singleHutsAllowed) {
			for (int yIndex = 0; yIndex < tiles.GetLength(1); yIndex += 2) {
				for (int xIndex = 0; xIndex < tiles.GetLength(2); xIndex += 2) {
					bool hasOneSurrounding = HexHelp.CheckDirRaw (xIndex - 1, yIndex, layerPos, tiles) ||
						HexHelp.CheckDirRaw (xIndex + 1, yIndex, layerPos, tiles) ||
						HexHelp.CheckDirRaw (xIndex, yIndex - 1, layerPos, tiles) ||
						HexHelp.CheckDirRaw (xIndex, yIndex + 1, layerPos, tiles);
					if (!hasOneSurrounding)
						tiles [layerPos, yIndex, xIndex] = HexType.None;
				}
			}
		}
	}

	public void FillHoles(int layerPos, HexType[,,] tiles) {
		for (int index = 0; index < 3; index += 1) {
			for (int yIndex = 1; yIndex < tiles.GetLength(1); yIndex += 2) {
				for (int xIndex = 1; xIndex < tiles.GetLength(2); xIndex += 2) {
					bool left = HexHelp.CheckDirRaw (xIndex - 1, yIndex, layerPos, tiles);
					bool right = HexHelp.CheckDirRaw (xIndex + 1, yIndex, layerPos, tiles);
					bool top = HexHelp.CheckDirRaw (xIndex, yIndex - 1, layerPos, tiles);
					bool bottom = HexHelp.CheckDirRaw (xIndex, yIndex + 1, layerPos, tiles);
					if (left && right && top && bottom) {
						tiles [layerPos, yIndex, xIndex] = HexType.StoneFoundation;
					} else if (left && right && top) {
						tiles [layerPos, yIndex, xIndex] = HexType.StoneFoundation;
						tiles [layerPos, yIndex - 1, xIndex] = HexType.StoneFoundation;
					} else if (left && right && bottom) {
						tiles [layerPos, yIndex, xIndex] = HexType.StoneFoundation;
						tiles [layerPos, yIndex + 1, xIndex] = HexType.StoneFoundation;
					} else if (left && top && bottom) {
						tiles [layerPos, yIndex, xIndex] = HexType.StoneFoundation;
						tiles [layerPos, yIndex, xIndex + 1] = HexType.StoneFoundation;
					} else if (right && top && bottom) {
						tiles [layerPos, yIndex, xIndex] = HexType.StoneFoundation;
						tiles [layerPos, yIndex, xIndex - 1] = HexType.StoneFoundation;
					}
				}
			}
		}
	}

	public void RemoveIfNoBottom(int layerPos, HexType[,,] tiles) {
		for (int yIndex = 0; yIndex < tiles.GetLength(1); yIndex += 1) {
			for (int xIndex = 0; xIndex < tiles.GetLength(2); xIndex += 1) {
				if (!HexHelp.CheckBelow(xIndex,yIndex,layerPos,tiles)) {
					if (tiles[layerPos, yIndex, xIndex] == HexType.StoneFoundation)
						tiles[layerPos, yIndex, xIndex] = HexType.None;
					else if (tiles[layerPos, yIndex, xIndex] == HexType.StoneConnectTemp) {
						if (SurroundedHorzOrVert(xIndex, yIndex, layerPos, tiles))
							tiles[layerPos, yIndex, xIndex] = HexType.StoneFoundation;//TODO Fix this
						else
							tiles[layerPos, yIndex, xIndex] = HexType.None;
					}
				} else if (tiles[layerPos, yIndex, xIndex] == HexType.StoneConnectTemp){
					tiles[layerPos, yIndex, xIndex] = HexType.StoneFoundation;
				}
			}
		}
	}

	public void DeleteFramesAndDoors(GameObject ceiling, int cityLayer, int layerPos, int yIndex, int xIndex, HexType[,,] tiles) {
		Vector2 point = new Vector2 (xIndex, yIndex + 1);
		
		for (int direction = 0; direction < 6; direction++) {
			bool fromStairs = (cityLayer > 0) && HexHelp.CheckForStairs((int)point.x, (int)point.y, layerPos, tiles);
			if (Random.value <= openDoorChance || fromStairs) {
				GameObject.DestroyImmediate( ceiling.transform.Find(doorFrameNames[direction]).GetChild(0).gameObject );
			}


			//Delete frames if neighboring
			if (HexHelp.CheckDirRaw((int)point.x, (int)point.y, layerPos, tiles) && !HexHelp.CheckForStairs((int)point.x, (int)point.y, layerPos, tiles)) {
				GameObject.Destroy( ceiling.transform.Find(doorFrameNames[direction]).gameObject );
			}
			
			point = HexHelp.MoveInDirection (point, (HexDir)direction);
		}

	}

	//TODO Move into HexHelp
	bool SurroundedHorzOrVert(int xIndex, int yIndex, int layerPos, HexType[,,] tiles) {
		bool leftAndRight = HexHelp.CheckDirRaw(xIndex - 1, yIndex, layerPos, tiles) && HexHelp.CheckDirRaw(xIndex + 1, yIndex, layerPos, tiles);
		bool topAndBottom = HexHelp.CheckDirRaw(xIndex, yIndex - 1, layerPos, tiles) && HexHelp.CheckDirRaw(xIndex, yIndex + 1, layerPos, tiles);
		return (leftAndRight || topAndBottom);
	}
}