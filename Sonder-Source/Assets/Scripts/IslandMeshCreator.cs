using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//NOTE: This script is both unfinished and unnused.

[System.Serializable]
public class IslandMeshCreator {

	List<Vector3> vertices = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();	
	List<Vector2> UVs = new List<Vector2>();
	List<int> indices = new List<int>();
	int radius;
	public PerlinLayer baseHoleLayer;
	public PerlinLayer baseHeightLayer;



	public Mesh GenerateMesh(int radius) {
		this.radius = radius;
		SetupBaseGrid ();
		SetupBaseQuads ();
		return FinalizeMesh ();

	}

	void SetupBaseGrid() {
		Vector3 realPos = new Vector3(-radius + 1, 0, -radius + 1);
		for (int yIndex = 0; yIndex < radius; yIndex += 1) {
			realPos.x = -radius + 1;
			for (int xIndex = 0; xIndex < radius; xIndex += 1) {
				vertices.Add(realPos + Vector3.up*baseHeightLayer.GetAt(realPos.x, realPos.z));	
				realPos.x += 2;
			}
			realPos.z += 2;
		}
	}

	void SetupBaseQuads() {
		float rSquared = radius * radius;
		for (int yIndex = 0; yIndex < radius - 1; yIndex += 1) {
			for (int xIndex = 0; xIndex < radius - 1; xIndex += 1) {
				//TODO Perhaps just use first value
				//TODO This whole bit could probably be optimised
				//TODO Add for different tiles
				Vector3 faceCenter = (vertices[IndexFromCoord(xIndex, yIndex)] + vertices[IndexFromCoord(xIndex+1, yIndex)] +
					vertices[IndexFromCoord(xIndex, yIndex+1)] + vertices[IndexFromCoord(xIndex+1, yIndex+1)])/4;				
				float distValue = Mathf.Min(0, 1 - (faceCenter.x * faceCenter.x + faceCenter.z * faceCenter.z) / rSquared);
				float holeValue = baseHoleLayer.GetAt(faceCenter.x, faceCenter.z);
				if (distValue + 1 * holeValue > baseHoleLayer.threshold) {
					SetupQuadShortestDiag(IndexFromCoord(xIndex, yIndex),IndexFromCoord(xIndex+1, yIndex), 
				                      	IndexFromCoord(xIndex, yIndex+1), IndexFromCoord(xIndex+1, yIndex+1));
				}
			}
		}
	}

	Mesh FinalizeMesh()
	{
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray();
		mesh.normals = normals.ToArray();
		mesh.uv = UVs.ToArray();
		mesh.triangles = indices.ToArray();
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
		return mesh;
	}
	
				                   

	int IndexFromCoord(int x, int y)
	{
		return x + y * (radius);
	}

	void SetupQuadShortestDiag(int a, int b, int c, int d) {
		float d1 = (vertices[a].x - vertices[d].x)*(vertices[a].x - vertices[d].x) + (vertices[a].z - vertices[d].z)*(vertices[a].z - vertices[d].z);
		float d2 = (vertices[b].x - vertices[c].x)*(vertices[b].x - vertices[c].x) + (vertices[b].z - vertices[c].z)*(vertices[b].z - vertices[c].z);
		if ( d1 < d2){
			indices.Add (d);
			indices.Add (a);
			indices.Add (c);
			
			indices.Add (d);
			indices.Add (b);
			indices.Add (a);
		} else {
			indices.Add (c);
			indices.Add (b);
			indices.Add (a);
			
			indices.Add (c);
			indices.Add (d);
			indices.Add (b);
		}
	}




}
