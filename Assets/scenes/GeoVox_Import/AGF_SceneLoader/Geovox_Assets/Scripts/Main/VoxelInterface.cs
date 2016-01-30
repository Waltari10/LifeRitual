using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Geovox{
	public class VoxelInterface : MonoBehaviour {

		public GameObject geovoxPrefab;
		public GeovoxData data;

		AGF_TerrainManager terrainManager;

		public Material grassMaterial;
		public Mesh grassMesh;
		public Texture2D grassWind;

		List<Transform> defaultPrefabs = new List<Transform>();

		void Start(){
			terrainManager = FindObjectOfType<AGF_TerrainManager> ();
		}
		
		Texture GetVoxelPaint(string textureName, Texture prevTexture){
			if (textureName == "null")
				return null;

			if(textureName == "")
				return prevTexture;

			if (textureName.EndsWith ("_s")) {
				textureName = textureName.Substring (0, textureName.Length - 2);
				textureName += "_c";
			}

			terrainManager = FindObjectOfType<AGF_TerrainManager> ();

			foreach (KeyValuePair<string,Texture2D> texture in terrainManager.GetLoadedColormaps ()) {
				if(texture.Value.name == textureName){
					return texture.Value;
				}
			}
			foreach (KeyValuePair<string,Texture2D> texture in terrainManager.GetLoadedNormalmaps()) {
				if(texture.Value.name == textureName){
					return texture.Value;
				}
			}
			foreach (Texture2D tex in terrainManager.defaultTextures) {
				if (tex.name == textureName) {
					return tex;
				}
			}

			Debug.LogError ("Could not find terrain " + textureName + " in loaded asset bundles");
			return prevTexture;
		}

		Texture GetVoxelGrass(string textureName, Texture prevTexture){
			if (!terrainManager)
				terrainManager = FindObjectOfType<AGF_TerrainManager> ();


			foreach (KeyValuePair<string,Texture2D> texture in terrainManager.GetLoadedVegetationTextures ()) {
				if(texture.Value.name == textureName){
					return texture.Value;
				}
			}

			return prevTexture;
		}

		public void SaveVoxelMesh(string path){
		}
		
		
		public void SetSettings(string settingsString){
			VoxelTerrain voxelTerrain = FindObjectOfType<VoxelTerrain> ();
			if (voxelTerrain.grass == null)
				voxelTerrain.grass = new VoxelGrassType[0];

			Color newColor = new Color ();
			float newFloat = 0;
			
			string[] settingsSplit = settingsString.Split (new char[]{'@'});
			int index = -1;
			
			int.TryParse (settingsSplit [++index], out voxelTerrain.brushSize);
			voxelTerrain.brushSphere = settingsSplit [++index] == "1";
			
			string[] typeListSplit = settingsSplit [++index].Split (new char[]{'%'});
			
			for (int t=0; t<typeListSplit.Length; t++) {
				if (t >= voxelTerrain.types.Length - 1) {
					Geovox.VoxelBlockType[] newTypeList = new Geovox.VoxelBlockType[voxelTerrain.types.Length + 1];
					for (int i = 0; i < voxelTerrain.types.Length; i++) {
						newTypeList [i] = voxelTerrain.types [i];
					}
					newTypeList [newTypeList.Length - 1] = new Geovox.VoxelBlockType ("Voxel (Terrain) or Prefab (Asset)", false);
					voxelTerrain.types = newTypeList;
				}
				
				Geovox.VoxelBlockType type = voxelTerrain.types [t + 1];
				string[] typeSplit = typeListSplit [t].Split (new char[]{'#'});
				int typeIndex = -1;
				
				type.name = typeSplit [++typeIndex];
				type.filledTerrain = typeSplit [++typeIndex] == "1";
				
				float.TryParse (typeSplit [++typeIndex], out type.color.r);
				float.TryParse (typeSplit [++typeIndex], out type.color.g);
				float.TryParse (typeSplit [++typeIndex], out type.color.b);
				float.TryParse (typeSplit [++typeIndex], out type.color.a);
				
				
				
				type.texture = GetVoxelPaint (typeSplit [++typeIndex], type.texture);
				type.bumpTexture = GetVoxelPaint (typeSplit [++typeIndex], type.bumpTexture);
				type.specGlossMap = GetVoxelPaint (typeSplit [++typeIndex], type.specGlossMap);
				
				float.TryParse (typeSplit [++typeIndex], out type.specular.r);
				float.TryParse (typeSplit [++typeIndex], out type.specular.g);
				float.TryParse (typeSplit [++typeIndex], out type.specular.b);
				float.TryParse (typeSplit [++typeIndex], out type.specular.a);
				
				float.TryParse (typeSplit [++typeIndex], out type.tile);
				
				type.grass = typeSplit [++typeIndex] == "1";
				type.filledPrefabs = typeSplit [++typeIndex] == "1";
				
				string[] prefabSplit = typeSplit [++typeIndex].Split (new char[]{'$'});
				for (int p = 0; p < prefabSplit.Length; p++) {
					if (prefabSplit [p] == "")
						continue;
					string[] prefabSplit2 = prefabSplit [p].Split (new char[]{'*'});
					int prefabIndex = 0;
					
					if (p >= type.prefabs.Length) {
						voxelTerrain.AddPrefab (type);  
					}
					do{
						type.offset.Add(0);
					}while(type.offset.Count < type.prefabs.Length);
					do{
						type.alignToNormals.Add(false);
					}while(type.alignToNormals.Count < type.prefabs.Length);
					do{
						type.objectNormalArea.Add(0);
					}while(type.objectNormalArea.Count < type.prefabs.Length);
					do{
						type.randomRotation.Add(true);
					}while(type.randomRotation.Count < type.prefabs.Length);
					do{
						type.scaleRamp.Add(0);
					}while(type.scaleRamp.Count < type.prefabs.Length);
					do{
						type.collidersActive.Add(true);
					}while(type.collidersActive.Count < type.prefabs.Length);
					
					if (prefabSplit2 [0] == "null") {
						type.prefabs [p] = null;
					} else if (prefabSplit2 [0] != "") {
						type.prefabs [p] = FindObjectOfType<AGF_TileListManager> ().GetTile (prefabSplit2 [0]);
						if (type.prefabs [p] == null)
							type.prefabs [p] = GetDefaultPrefabs (prefabSplit2 [0]);
					}
					float newOffset = 0;
					float.TryParse (prefabSplit2 [++prefabIndex], out newOffset);
					type.offset [p] = newOffset;
					
					type.alignToNormals [p] = prefabSplit2 [++prefabIndex] == "1";
					
					float newNormalArea = 0;
					float.TryParse (prefabSplit2 [++prefabIndex], out newNormalArea);
					type.objectNormalArea [p] = newNormalArea;
					
					if (prefabSplit2.Length > prefabIndex + 1){
						type.randomRotation [p] = prefabSplit2 [++prefabIndex] == "1";
					}
					
					float newScaleRamp = 0;
					if (prefabSplit2.Length > prefabIndex + 1)
						float.TryParse (prefabSplit2 [++prefabIndex], out newScaleRamp);
					type.scaleRamp [p] = newScaleRamp;
					
					if (prefabSplit2.Length > prefabIndex + 1)
						type.collidersActive [p] = prefabSplit2 [++prefabIndex] == "1";
					else
						type.collidersActive [p] = true;
				}
				
				type.filledAmbient = typeSplit [++typeIndex] == "1";
			}
			
			
			string[] grassListSplit = settingsSplit [++index].Split (new char[]{'%'});
			
			for (int t=0; t< grassListSplit.Length; t++) {
				if(t >= voxelTerrain.grass.Length - 1){
					VoxelGrassType[] newGrassList = new VoxelGrassType[voxelTerrain.grass.Length + 1];
					for (int i = 0; i < voxelTerrain.grass.Length; i++) {
						newGrassList [i] = voxelTerrain.grass [i];
					}
					voxelTerrain.grass = newGrassList;
					
					voxelTerrain.grass[voxelTerrain.grass.Length - 1] = new VoxelGrassType("New");
					voxelTerrain.grass[voxelTerrain.grass.Length - 1].material = new Material(grassMaterial);
					voxelTerrain.grass[voxelTerrain.grass.Length - 1].sourceMesh = grassMesh;
					voxelTerrain.grass[voxelTerrain.grass.Length - 1].material.EnableKeyword("_Wind");
					voxelTerrain.grass[voxelTerrain.grass.Length - 1].material.SetTexture("_WindTex", grassWind);
				}
				if(t >= voxelTerrain.grass.Length - 1){
					VoxelGrassType[] newGrassList = new VoxelGrassType[voxelTerrain.grass.Length + 1];
					for (int i = 0; i < voxelTerrain.grass.Length; i++) {
						newGrassList [i] = voxelTerrain.grass [i];
					}
					voxelTerrain.grass = newGrassList;
					
					voxelTerrain.grass[voxelTerrain.grass.Length - 1] = new VoxelGrassType("New");
					voxelTerrain.grass[voxelTerrain.grass.Length - 1].material = new Material(grassMaterial);
					voxelTerrain.grass[voxelTerrain.grass.Length - 1].sourceMesh = grassMesh;
					voxelTerrain.grass[voxelTerrain.grass.Length - 1].material.EnableKeyword("_Wind");
					voxelTerrain.grass[voxelTerrain.grass.Length - 1].material.SetTexture("_WindTex", grassWind);
				}
				
				VoxelGrassType type = voxelTerrain.grass [t+1];
				string[] typeSplit = grassListSplit [t].Split (new char[]{'#'});
				int typeIndex = -1;
				
				type.name = typeSplit[++typeIndex];
				type.material.mainTexture = GetVoxelGrass(typeSplit[++typeIndex], type.material.mainTexture);
				type.normalsFromTerrain = typeSplit[++typeIndex] == "1";
				
				
				float.TryParse(typeSplit[++typeIndex], out newColor.r);
				float.TryParse(typeSplit[++typeIndex], out newColor.g);
				float.TryParse(typeSplit[++typeIndex], out newColor.b);
				float.TryParse(typeSplit[++typeIndex], out newColor.a);
				type.material.SetColor("_Color", newColor);
				
				float.TryParse(typeSplit[++typeIndex], out newFloat);
				type.material.SetFloat("_Cutoff", newFloat);
				
				float.TryParse(typeSplit[++typeIndex], out newColor.r);
				float.TryParse(typeSplit[++typeIndex], out newColor.g);
				float.TryParse(typeSplit[++typeIndex], out newColor.b);
				float.TryParse(typeSplit[++typeIndex], out newColor.a);
				type.material.SetColor("_Specular", newColor);
				
				float.TryParse(typeSplit[++typeIndex], out newFloat);
				type.material.SetFloat("_AmbientPower", newFloat);
				float.TryParse(typeSplit[++typeIndex], out newFloat);
				type.material.SetFloat("_WindSpeedX", newFloat);
				float.TryParse(typeSplit[++typeIndex], out newFloat);
				type.material.SetFloat("_WindSpeedZ", newFloat);
				float.TryParse(typeSplit[++typeIndex], out newFloat);
				type.material.SetFloat("_WindStrength", newFloat);
			}

			GameObject areaMarker = new GameObject ();
			areaMarker.name = "AreaMarker";
			Vector3 area = Vector3.zero;
			if (settingsSplit.Length > index + 1)
				float.TryParse (settingsSplit [++index], out area.x);
			else
				area.x = 150;
			if(settingsSplit.Length > index + 1) float.TryParse(settingsSplit[++index], out area.y);
			if (settingsSplit.Length > index + 1)
				float.TryParse (settingsSplit [++index], out area.z);
			else
				area.z = 150;
			areaMarker.transform.position = area;

			++index;

			if (settingsSplit.Length > index + 1) {
				int.TryParse (settingsSplit [++index], out voxelTerrain.guiAreaSize);
			}
		}

		Transform GetDefaultPrefabs(string prefabString){
			foreach (Transform prefab in defaultPrefabs) {
				if(prefab.name == prefabString)
					return prefab;
			}
			return null;
		}
	}
}