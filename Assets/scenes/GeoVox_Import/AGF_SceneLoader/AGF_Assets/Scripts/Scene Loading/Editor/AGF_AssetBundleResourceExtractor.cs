using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class AGF_AssetBundleResourceExtractor : MonoBehaviour {
	
	public static string sourceFolder = "Assets/AGF_SourceAssets"; // May be changed by the user.
	public static string sceneFolder = ""; // To be set by the tool.
	
	public static void ExtractSkybox(){

	}
	
	private static string[] colorMaps = new string[]{ "splatA_c.asset", "splatB_c.asset", "splatC_c.asset", "splatD_c.asset", "splatE_c.asset" };
	private static string[] normalMaps = new string[]{ "splatA_n.asset", "splatB_n.asset", "splatC_n.asset", "splatD_n.asset", "splatE_n.asset" };
	public static void ExtractTerrainTextures(){
		AGF_TerrainManager terrainManager = GameObject.Find ("AGF_TerrainManager").GetComponent<AGF_TerrainManager>();
		
		// step 1: create the target directory, if necessary.
		string targetDirectory = Main.TrimEndFromString( Application.dataPath, "Assets" ) + sourceFolder + "/" + sceneFolder + "/Terrain";
		if ( Directory.Exists( targetDirectory ) == false ){
			Directory.CreateDirectory( targetDirectory );
		}
		
		// step 2: clear the old files, if they exist.
		for ( int i = 0; i < 5; i++ ){
			if ( File.Exists( targetDirectory + "/" + colorMaps[i] ) ){
				AssetDatabase.DeleteAsset( sourceFolder + "/" + sceneFolder + "/Terrain/" + colorMaps[i] );
			}
			if ( File.Exists ( targetDirectory + "/" + normalMaps[i] ) ){
				AssetDatabase.DeleteAsset( sourceFolder + "/" + sceneFolder + "/Terrain/" + normalMaps[i] );
			}
		}
		
		if ( File.Exists( targetDirectory + "/TriplanarMaterial.mat" ) ){
			AssetDatabase.DeleteAsset( sourceFolder + "/" + sceneFolder + "/Terrain/TriplanarMaterial.mat" );	
		}
		
		// step 3: copy the terrain template.
		TerrainData template = (TerrainData)AssetDatabase.LoadAssetAtPath( "Assets/AGF_SceneLoader/AGF_Assets/Resources/Terrain/TerrainTemplate.asset", typeof(TerrainData) );
		if ( template == null ){
			Debug.LogError ("Template was null :(");			
		} else {
			TerrainData newTemplate = new TerrainData();
			EditorUtility.CopySerialized( template, newTemplate );
			AssetDatabase.CreateAsset( newTemplate, sourceFolder + "/" + sceneFolder + "/Terrain/" + "TerrainTemplate.asset" );
//			AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Terrain/" + "TerrainTemplate.asset" ); 
			TerrainData importedTemplate = (TerrainData)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Terrain/" + "TerrainTemplate.asset", typeof(TerrainData) );
			if ( importedTemplate == null ){
				Debug.LogError ("Imported template was null :(");				
			} else {
				terrainManager.GetCurrentTerrainInstance().terrainData = importedTemplate;
				terrainManager.GetCurrentTerrainInstance().GetComponent<TerrainCollider>().terrainData = importedTemplate;
			}
		}
			
		// step 4: copy the textures from the terrrain into new temporary textures, and save them out as pngs.
		for ( int i = 0; i < 5; i++ ){
			Texture2D cRef = terrainManager.GetTerrainImage(i);
			Texture2D colorMap = new Texture2D( cRef.width, cRef.height, TextureFormat.RGB24, true );
			EditorUtility.CopySerialized( cRef, colorMap );
			AssetDatabase.CreateAsset( colorMap, sourceFolder + "/" + sceneFolder + "/Terrain/" + colorMaps[i] );
			
			Texture2D nRef = terrainManager.GetTerrainNormal(i);
			Texture2D normalMap = new Texture2D( nRef.width, nRef.height, TextureFormat.RGB24, true );
			EditorUtility.CopySerialized( nRef, normalMap );
			AssetDatabase.CreateAsset( normalMap, sourceFolder + "/" + sceneFolder + "/Terrain/" + normalMaps[i] );
			
//			AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Terrain/" + colorMaps[i] ); 
//			AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Terrain/" + normalMaps[i] ); 
			
			terrainManager.SetTexture ( i, 
				(Texture2D)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Terrain/" + colorMaps[i], typeof(Texture2D) ),
				(Texture2D)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Terrain/" + normalMaps[i],typeof(Texture2D) ),
				"" );
		}
		
		// step 5: copy the detail textures from the terrain, and save them out as pngs.
		for ( int i = 0; i < terrainManager.GetDetailLayerCount(); i++ ){
			Texture2D dRef = terrainManager.GetDetailLayer(i).image;
			Texture2D detailMap = new Texture2D( dRef.width, dRef.height, TextureFormat.ARGB32, true );
			EditorUtility.CopySerialized( dRef, detailMap );
			AssetDatabase.CreateAsset( detailMap, sourceFolder + "/" + sceneFolder + "/Terrain/Detail_" + i + ".asset" );
//			AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Terrain/Detail_" + i + ".asset" );
			
			terrainManager.SetVegetationTexture( i, (Texture2D)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Terrain/Detail_" + i + ".asset", typeof(Texture2D) ), "" );
		}
		
		// step 6: copy the terrain material, and save it.
		Material triplanarMat = new Material( terrainManager.terrainMaterial );
		EditorUtility.CopySerialized( terrainManager.terrainMaterial, triplanarMat );
		
		AssetDatabase.CreateAsset( triplanarMat, sourceFolder + "/" + sceneFolder + "/Terrain/TriplanarMaterial.mat" );
//		AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Terrain/TriplanarMaterial.mat" );
		terrainManager.SetTerrainMaterial( (Material)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Terrain/TriplanarMaterial.mat", typeof(Material) ) );
		
		EditorUtility.SetDirty( terrainManager );
	}
	
	public static void InitWarehouseDirectories(){
		string targetDirectory = Main.TrimEndFromString( Application.dataPath, "Assets" ) + sourceFolder + "/" + sceneFolder + "/Warehouse";
		if ( Directory.Exists( targetDirectory ) == false ){
			Directory.CreateDirectory( targetDirectory );
		}
		
		if ( Directory.Exists( targetDirectory + "/Meshes" ) == true ){
			Directory.Delete( targetDirectory + "/Meshes", true );
		}
		Directory.CreateDirectory( targetDirectory + "/Meshes" );
		
		if ( Directory.Exists( targetDirectory + "/Materials" ) == true ){
			Directory.Delete( targetDirectory + "/Materials", true );
		}
		Directory.CreateDirectory( targetDirectory + "/Materials" );
		
		if ( Directory.Exists ( targetDirectory + "/Textures" ) == true ){
			Directory.Delete( targetDirectory + "/Textures", true );
		}
		Directory.CreateDirectory ( targetDirectory + "/Textures" );
		
		if ( Directory.Exists (Main.TrimEndFromString( Application.dataPath, "Assets" ) + sourceFolder + "/" + sceneFolder + "/Voxels" ) == true ){
			Directory.Delete( Main.TrimEndFromString( Application.dataPath, "Assets" ) + sourceFolder + "/" + sceneFolder + "/Voxels", true );
		}
		Directory.CreateDirectory ( Main.TrimEndFromString( Application.dataPath, "Assets" ) + sourceFolder + "/" + sceneFolder + "/Voxels" );
	}
	
	private static Dictionary<Mesh, string> savedMeshList;
	private static Dictionary<Material, string> savedMaterialList;
	private static Dictionary<Texture2D, string> savedTextureList;
	private static List<Transform> warehouseObjectList;
	public static void InitWarehouseExtractionLists(){
		Debug.Log ("Init Warehouse Extraction Lists");
		savedMeshList = new Dictionary<Mesh, string>();
		savedMaterialList = new Dictionary<Material, string>();
		savedTextureList = new Dictionary<Texture2D, string>();
		
		warehouseObjectList = new List<Transform>();
		
		Dictionary<int, Transform> placedTileList = GameObject.Find ("AGF_GridManager").GetComponent<AGF_GridManager>().GetPlacedTileList();
		foreach ( KeyValuePair<int, Transform> pair in placedTileList ){
			warehouseObjectList.Add ( pair.Value );	
		}
	}
	
	public static int GetNumberOfWarehouseObjects(){
		return warehouseObjectList.Count;
	}
	
	public static bool ExtractWarehouseObject( Transform tile ){

		if (tile == null) {
			Debug.LogError("Tile is null");
			return true;
		}

		if (!Physics.Raycast (tile.position + Vector3.up * 5, Vector3.down, 10)) {
			DestroyImmediate(tile.gameObject);
			return true;
		}

		string targetDirectory = Main.TrimEndFromString( Application.dataPath, "Assets" ) + "/" + sourceFolder + "/" + sceneFolder + "/Warehouse";

		if (!Directory.Exists(targetDirectory))
			Directory.CreateDirectory (targetDirectory);
		
		// Get the tile and related names.
//		TileProperties tileProperties = tile.GetComponent<TileProperties>();
//		string[] split = tileProperties.tileID.Split( new char[]{'/'} );
		//		string packName = split[0];
//		string tileName = split[1];
		
		// part A: SCRIPTS
		Component[] scripts = tile.GetComponentsInChildren<MonoBehaviour>();
		
		// remove the scripts.
		for ( int i = 0; i < scripts.Length; i++ ){
			DestroyImmediate( scripts[i] );	
		}
		
		// part B: MESH FILTERS
		MeshFilter[] objectMeshFilters = tile.GetComponentsInChildren<MeshFilter>();

		if (!AGF_IntegrationWindow.prefabShadows) {
			foreach(MeshFilter filter in objectMeshFilters){
				if(filter.GetComponent<Renderer>())
					filter.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
		}
		
		for ( int i = 0; i < objectMeshFilters.Length; i++ ){
			Mesh sharedMesh = objectMeshFilters[i].sharedMesh;
			if(sharedMesh==null)
				break;
			
			// create the meshes, if necessary.
			if ( savedMeshList.ContainsKey( sharedMesh ) == false ){
				Mesh newMesh = new Mesh();
				//				EditorUtility.CopySerialized( sharedMesh, newMesh );
				newMesh.subMeshCount = sharedMesh.subMeshCount;
				newMesh.vertices = sharedMesh.vertices;
				for(int subMeshIndex = 0; subMeshIndex< newMesh.subMeshCount; subMeshIndex++){
					newMesh.SetTriangles(sharedMesh.GetTriangles(subMeshIndex), subMeshIndex);
				}
				
				newMesh.uv = sharedMesh.uv;
				newMesh.uv2 = sharedMesh.uv2;
				newMesh.uv3 = sharedMesh.uv3;
				newMesh.uv4 = sharedMesh.uv4;
				newMesh.normals = sharedMesh.normals;
				newMesh.tangents = sharedMesh.tangents;
				
				int count = 1;
				
				string meshPath = "Meshes/" + sharedMesh.name + count.ToString() + ".asset";

				while ( File.Exists( targetDirectory + "/" + meshPath ) ){
					// a different version of the mesh already exists. we'll need to rename this one.
					meshPath = "Meshes/" + sharedMesh.name + count.ToString() + ".asset";	
					count++;
				}
				
				// ensure that the meshPath string has no invalid characters.
				meshPath = RemoveInvalidPathCharacters( meshPath );
				
				AssetDatabase.CreateAsset( newMesh, sourceFolder + "/" + sceneFolder + "/Warehouse/" + meshPath );
				//				AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Warehouse/" + meshPath );
				
				savedMeshList.Add ( sharedMesh, meshPath );
			}
			
			// save the mesh reference back into the object.
			objectMeshFilters[i].sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Warehouse/" + savedMeshList[sharedMesh], typeof(Mesh) );
		}
		
		// part C: MESH COLLIDERS
		MeshCollider[] objectMeshColliders = tile.GetComponentsInChildren<MeshCollider>();
		
		for ( int i = 0; i < objectMeshColliders.Length; i++ ){
			Mesh sharedMesh = objectMeshColliders[i].sharedMesh;
			string meshPath = "Meshes/" + sharedMesh.name + ".asset";
			
			// create the meshes, if necessary.
			if ( savedMeshList.ContainsKey( sharedMesh ) == false ){
				Mesh newMesh = new Mesh();
				EditorUtility.CopySerialized( sharedMesh, newMesh );
				int count = 1;
				while ( File.Exists( targetDirectory + "/" + meshPath ) ){
					// a different version of the mesh already exists. we'll need to rename this one.
					meshPath = "Meshes/" + sharedMesh.name + count.ToString() + ".asset";	
					count++;
				}
				
				// ensure that the meshPath string has no invalid characters.
				meshPath = RemoveInvalidPathCharacters( meshPath );
				
				AssetDatabase.CreateAsset( newMesh, sourceFolder + "/" + sceneFolder + "/Warehouse/" + meshPath );
				//				AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Warehouse/" + meshPath );
				
				savedMeshList.Add ( sharedMesh, meshPath );
			}
			
			// save the mesh reference back into the object.
			objectMeshColliders[i].sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Warehouse/" + savedMeshList[sharedMesh], typeof(Mesh) );
		}
		
		// part D: MATERIALS and TEXTURES
		Renderer[] objRenderers = tile.GetComponentsInChildren<Renderer>();
		for ( int i = 0; i < objRenderers.Length; i++ ){
			Material[] rendererMaterials = objRenderers[i].sharedMaterials;

			
			
			for ( int j = 0; j < rendererMaterials.Length; j++ ){

				Material sharedMaterial = rendererMaterials[j];
				if(sharedMaterial==null)
					break;

				string materialPath = "Materials/" + sharedMaterial.name + ".mat";
				
				
				// create the materials, if necessary.
				if ( savedMaterialList.ContainsKey( sharedMaterial ) == false ){
					Material newMat = new Material( sharedMaterial );
					EditorUtility.CopySerialized( sharedMaterial, newMat );
					
					int count = 1;
					while ( File.Exists ( targetDirectory + "/" + materialPath ) ){
						// a different version of the material already exists. we'll need to rename this one.
						materialPath = "Materials/" + sharedMaterial.name + count.ToString() + ".mat";	
						count++;
					}


					
					// before saving out this material (we now know the name is unique, and valid), we need to save out the textures that the material refers to.
					Dictionary<string,string> texturePaths = ExtractTexturesFromMaterial( sharedMaterial, targetDirectory, savedTextureList, !sharedMaterial.HasProperty("_MainTex") );
					
					if ( newMat == null ){
						print ("Material was null, reattempting...");
						return false;
					}
					foreach( KeyValuePair<string,string> textureInfo in texturePaths ){ 
						newMat.SetTexture( textureInfo.Key, (Texture2D)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Warehouse/" + textureInfo.Value, typeof(Texture2D) ) );
					}
					
					// save the shader reference directly.
					newMat.shader = Shader.Find( sharedMaterial.shader.name );
					
					// ensure that the materialPath string has no invalid characters.
					materialPath = RemoveInvalidPathCharacters( materialPath );

					if(!Directory.Exists(Main.GetHomeFolder() + "/" + sourceFolder + "/" + sceneFolder + "/Warehouse")){
						Directory.CreateDirectory(Main.GetHomeFolder() + "/" + sourceFolder + "/" + sceneFolder + "/Warehouse");
					}

					AssetDatabase.CreateAsset( newMat, sourceFolder + "/" + sceneFolder + "/Warehouse/" + materialPath );
					//					AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Warehouse/" + materialPath );
					
					objRenderers[i].sharedMaterials[j] = newMat;
					
					savedMaterialList.Add ( sharedMaterial, materialPath );
				}
				
				// save the material reference back into the object.
				Material mat = (Material)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Warehouse/" + savedMaterialList[sharedMaterial], typeof(Material) );
				rendererMaterials[j] = mat;
				objRenderers[i].sharedMaterials = rendererMaterials;
			}
		}
		
		return true;
	}
	
	
	public static void ExtractVoxelObject( Transform tile ){
		
		string targetDirectory = Main.TrimEndFromString( Application.dataPath, "Assets" ) + sourceFolder + "/" + sceneFolder + "/Voxel";
		
		
		print ( "Extracting assets from: " + tile.name );
		
		
		if(!Directory.Exists(Main.GetHomeFolder() + "/" + sourceFolder + "/" + sceneFolder + "/Voxels")){
			Directory.CreateDirectory(Main.GetHomeFolder() + "/" + sourceFolder + "/" + sceneFolder + "/Voxels");
		}			
		if(!Directory.Exists(Main.GetHomeFolder() + "/" + sourceFolder + "/" + sceneFolder + "/Voxels/Materials")){
			Directory.CreateDirectory(Main.GetHomeFolder() + "/" + sourceFolder + "/" + sceneFolder + "/Voxels/Materials");
		}		
//		if(!Directory.Exists(Main.GetHomeFolder() + "/" + sourceFolder + "/" + sceneFolder + "/Voxels/Textures")){
//			Directory.CreateDirectory(Main.GetHomeFolder() + "/" + sourceFolder + "/" + sceneFolder + "/Voxels/Textures");
		//		}	

		Geovox.Chunk[] objectMeshFilters = tile.GetComponentsInChildren<Geovox.Chunk>(true);
		
		// part B: MESH FILTERS
		for ( int i = 0; i < objectMeshFilters.Length; i++ ){

			Mesh sharedMesh = objectMeshFilters[i].hiFilter.sharedMesh;
			if(sharedMesh==null)
				continue;
			string meshPath = "Voxels/GeovoxChunk" + i + ".asset";

			// create the meshes, if necessary.
			if ( savedMeshList.ContainsKey( sharedMesh ) == false ){
				Mesh newMesh = new Mesh();
				//				EditorUtility.CopySerialized( sharedMesh, newMesh );
				newMesh.vertices = sharedMesh.vertices;
				newMesh.triangles = sharedMesh.triangles;
				newMesh.uv = sharedMesh.uv;
				newMesh.uv2 = sharedMesh.uv2;
				newMesh.normals = sharedMesh.normals;
				newMesh.tangents = sharedMesh.tangents;

				
				// ensure that the meshPath string has no invalid characters.
				meshPath = RemoveInvalidPathCharacters( meshPath );
				
				if(!Directory.Exists(sourceFolder + "/" + sceneFolder + "/Voxels/"))
					Directory.CreateDirectory(sourceFolder + "/" + sceneFolder + "/Voxels/");
				
				AssetDatabase.CreateAsset( newMesh, sourceFolder + "/" + sceneFolder + "/"+ meshPath );
				//				AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Warehouse/" + meshPath );
				
				savedMeshList.Add ( sharedMesh, meshPath );
			}
			
			// save the mesh reference back into the object.
			objectMeshFilters[i].hiFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/" + meshPath, typeof(Mesh) );
			objectMeshFilters[i].hiFilter.gameObject.AddComponent<MeshCollider>();
			objectMeshFilters[i].hiFilter.GetComponent<MeshCollider>().sharedMesh = objectMeshFilters[i].hiFilter.GetComponent<MeshFilter>().sharedMesh;
		}
		
		
		// part D: MATERIALS and TEXTURES		
		for ( int i = 0; i < objectMeshFilters.Length; i++ ){

			Material[] rendererMaterials = objectMeshFilters[i].hiFilter.GetComponent<Renderer>().sharedMaterials;

			for ( int j = 0; j < rendererMaterials.Length; j++ ){
				Material sharedMaterial = rendererMaterials[j];
				if(sharedMaterial==null){
					break;
				}
				string materialPath = "Materials/" + sharedMaterial.name.Replace("/", "_") + ".mat";
				
				// create the materials, if necessary.
				if ( savedMaterialList.ContainsKey( sharedMaterial ) == false ){
					Material newMat = new Material( sharedMaterial );
					EditorUtility.CopySerialized( sharedMaterial, newMat );
					
					int count = 1;
					while ( File.Exists (sourceFolder + "/" + sceneFolder + "/Voxels/" + materialPath ) ){
						// a different version of the material already exists. we'll need to rename this one.
						materialPath = "Materials/" + sharedMaterial.name.Replace("/", "_") + count.ToString() + ".mat";	
						count++;
					}
					
					// before saving out this material (we now know the name is unique, and valid), we need to save out the textures that the material refers to.
					Dictionary<string,string> texturePaths = ExtractTexturesFromMaterial( sharedMaterial, targetDirectory, savedTextureList );
					
					if ( newMat == null ){
						print ("Material was null, reattempting...");
						return;
					}
					foreach( KeyValuePair<string,string> textureInfo in texturePaths ){ 
						newMat.SetTexture( textureInfo.Key, (Texture2D)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Warehouse/" + textureInfo.Value, typeof(Texture2D) ) );
					}
					
					// save the shader reference directly.
					newMat.shader = Shader.Find( sharedMaterial.shader.name );
					
					// ensure that the materialPath string has no invalid characters.
					materialPath = RemoveInvalidPathCharacters( materialPath );
					
					AssetDatabase.CreateAsset( newMat, sourceFolder + "/" + sceneFolder + "/Voxels/" + materialPath );
					//					AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Warehouse/" + materialPath );
					
					objectMeshFilters[i].hiFilter.GetComponent<Renderer>().sharedMaterials[j] = newMat;
					
					savedMaterialList.Add ( sharedMaterial, materialPath );
				}
				
				// save the material reference back into the object.
				Material mat = (Material)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Voxels/" + materialPath, typeof(Material) );


				rendererMaterials[j] = mat;
			}
			objectMeshFilters[i].hiFilter.GetComponent<Renderer>().sharedMaterials = rendererMaterials;

			foreach(Material mat in objectMeshFilters[i].hiFilter.GetComponent<Renderer>().sharedMaterials){

				if(!AGF_IntegrationWindow.terrainSpecular){
					mat.shader = Shader.Find("Voxel/Standard");
				}
				else{
					mat.shader = Shader.Find("Voxel/Standard (Specular setup)");
				}
			}
			
			if(!AGF_IntegrationWindow.terrainShadows)
				objectMeshFilters[i].hiFilter.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			else
				objectMeshFilters[i].hiFilter.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

			DestroyImmediate(objectMeshFilters[i].loFilter.gameObject);
		}
		
		for ( int i = 0; i < objectMeshFilters.Length; i++ ){

			Mesh sharedMesh = objectMeshFilters[i].grassFilter.sharedMesh;
			if(sharedMesh==null || sharedMesh.vertices.Length ==0){
				continue;
			}
			// create the meshes, if necessary.
			if ( savedMeshList.ContainsKey( sharedMesh ) == false ){
				if(sharedMesh.vertices.Length ==0)
					continue;

				Mesh newMesh = new Mesh();
				//				EditorUtility.CopySerialized( sharedMesh, newMesh );
				newMesh.vertices = sharedMesh.vertices;
				newMesh.triangles = sharedMesh.triangles;
				newMesh.uv = sharedMesh.uv;
				newMesh.uv2 = sharedMesh.uv2;
				newMesh.normals = sharedMesh.normals;
				newMesh.tangents = sharedMesh.tangents;
				
				string meshPath = "Voxels/GeovoxGrass" + i + ".asset";
				
				// ensure that the meshPath string has no invalid characters.
				meshPath = RemoveInvalidPathCharacters( meshPath );
				
				if(!Directory.Exists(sourceFolder + "/" + sceneFolder + "/Voxels/"))
					Directory.CreateDirectory(sourceFolder + "/" + sceneFolder + "/Voxels/");
				
				AssetDatabase.CreateAsset( newMesh, sourceFolder + "/" + sceneFolder + "/"+ meshPath );
				//				AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Warehouse/" + meshPath );
				
				savedMeshList.Add ( sharedMesh, meshPath );
			}
			
			// save the mesh reference back into the object.
			objectMeshFilters[i].grassFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Voxels/GeovoxGrass" + i + ".asset", typeof(Mesh) );
		}		

		for ( int i = 0; i < objectMeshFilters.Length; i++ ){

			Material[] rendererMaterials = objectMeshFilters[i].grassFilter.GetComponent<Renderer>().sharedMaterials;
			
			for ( int j = 0; j < rendererMaterials.Length; j++ ){
				Material sharedMaterial = rendererMaterials[j];
				if(sharedMaterial==null)
					break;
				string materialPath = "Materials/" + sharedMaterial.name.Replace("/", "_")+ ".mat";
				
				
				// create the materials, if necessary.
				if ( savedMaterialList.ContainsKey( sharedMaterial ) == false ){
					Material newMat = new Material( sharedMaterial );
					EditorUtility.CopySerialized( sharedMaterial, newMat );
					
					int count = 1;
					while ( File.Exists (sourceFolder + "/" + sceneFolder + "/Voxels/" + materialPath ) ){
						// a different version of the material already exists. we'll need to rename this one.
						materialPath = "Materials/" + sharedMaterial.name + count.ToString() + ".mat";	
						count++;
					}
					
					// before saving out this material (we now know the name is unique, and valid), we need to save out the textures that the material refers to.
					Dictionary<string,string> texturePaths = ExtractTexturesFromMaterial( sharedMaterial, targetDirectory, savedTextureList );
					
					if ( newMat == null ){
						print ("Material was null, reattempting...");
						return;
					}
					foreach( KeyValuePair<string,string> textureInfo in texturePaths ){ 
						newMat.SetTexture( textureInfo.Key, (Texture2D)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Warehouse/" + textureInfo.Value, typeof(Texture2D) ) );
					}
					
					// save the shader reference directly.
					newMat.shader = Shader.Find( sharedMaterial.shader.name );
					
					// ensure that the materialPath string has no invalid characters.
					materialPath = RemoveInvalidPathCharacters( materialPath );						
					
					AssetDatabase.CreateAsset( newMat, sourceFolder + "/" + sceneFolder + "/Voxels/" + materialPath );
					
					objectMeshFilters[i].grassFilter.GetComponent<Renderer>().sharedMaterials[j] = newMat;
					
					savedMaterialList.Add ( sharedMaterial, materialPath );
				}
				
				// save the material reference back into the object.
				Material mat = (Material)AssetDatabase.LoadAssetAtPath( sourceFolder + "/" + sceneFolder + "/Voxels/" + materialPath, typeof(Material) );
				rendererMaterials[j] = mat;
				objectMeshFilters[i].grassFilter.GetComponent<Renderer>().sharedMaterials = rendererMaterials;

				if(!AGF_IntegrationWindow.grassShadows)
					objectMeshFilters[i].grassFilter.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				else
					objectMeshFilters[i].grassFilter.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			}
		}

		RaycastHit hit;
		float areaSize = FindObjectOfType<Geovox.VoxelTerrain> ().guiAreaSize ;
		if (Physics.Raycast (new Vector3(areaSize, 1000,  areaSize), Vector3.down, out hit, 1000) && GameObject.Find("FPSController")) {
			GameObject.Find("FPSController").transform.position = hit.point + Vector3.up;
		}
		
		// SCRIPTS
		Component[] scripts = tile.GetComponentsInChildren<MonoBehaviour>();


		// remove the scripts.
		for ( int i = 0; i < scripts.Length; i++ ){
			DestroyImmediate( scripts[i] );	
		}
		DestroyImmediate (GameObject.Find ("Far"));
	}
	
	private static string[] commonTextureProperties = new string[]{
        "_AlphaTex",
        "_BumpMap",
        "_BackTex",
        "_Control",
        "_DecalTex",
        "_Detail",
        "_DownTex",
        "_FrontTex",
        "_LeftTex",
        "_LightMap",
		"_MainTex",
		"_MainTex2",
		"_MainTex3",
		"_MainTex4",
		"_RightTex",
        "_Splat0",
        "_Splat1",
        "_Splat2",
        "_Splat3",
        "_UpTex",
		// Voxel Triplanar
		"_MainTex2",
		"_BumpMap2",
		"_MainTex3",
		"_BumpMap3",
		"_MainTex4",
		"_BumpMap4"
    };
	private static Dictionary<string, string> ExtractTexturesFromMaterial( Material material, string targetDirectory, Dictionary<Texture2D, string> savedTextureList, bool hasPropertyOverride = false ){
		Dictionary<string, string> texturePaths = new Dictionary<string, string>();
			
		for ( int i = 0; i < commonTextureProperties.Length; i++ ){
			string textureProperty = commonTextureProperties[i];
			if (hasPropertyOverride)
				textureProperty = "_MainTex";
			
			if (hasPropertyOverride || (material.HasProperty(textureProperty) && material.GetTexture(textureProperty) != null) ){
				Texture2D oldTexture = (Texture2D)material.GetTexture(textureProperty);
				string texturePath = "Textures/" + oldTexture.name + ".asset";
				
				// create the textures, if necessary.
				if ( savedTextureList.ContainsKey( oldTexture ) == false ){
					Texture2D newTex = new Texture2D(oldTexture.width, oldTexture.height);
					EditorUtility.CopySerialized( oldTexture, newTex );
					int count = 1;
					while ( File.Exists ( targetDirectory + "/" + texturePath ) ||
					       File.Exists(sourceFolder + "/" + sceneFolder + "/Warehouse/" + texturePath)){
						// a different version of the material already exists. we'll need to rename this one.
						texturePath = "Materials/" + oldTexture.name + count.ToString() + ".asset";	
						count++;
					}
					
					// ensure that the texturePath string has no invalid characters.
					texturePath = RemoveInvalidPathCharacters( texturePath );

					AssetDatabase.CreateAsset( newTex, sourceFolder + "/" + sceneFolder + "/Warehouse/" + texturePath );
//					AssetDatabase.ImportAsset( sourceFolder + "/" + sceneFolder + "/Warehouse/" + texturePath );
					
					savedTextureList.Add ( oldTexture, texturePath );

					print(newTex.name +" saved to " +sourceFolder + "/" + sceneFolder + "/Warehouse/" + texturePath);
				}

				if(!texturePaths.ContainsKey(textureProperty))
					texturePaths.Add ( textureProperty, texturePath );
			}

			if (hasPropertyOverride)
				return texturePaths;
		}
		
		return texturePaths;
	}
	
	private static string RemoveInvalidPathCharacters( string filepath ){
		for ( int i = 0; i < Path.GetInvalidPathChars().Length; i++ ){
			filepath = filepath.Replace( Path.GetInvalidPathChars()[i].ToString(), "" );
		}
		
		filepath = filepath.Replace( ":", "" );
		
		return filepath;
	}
}
