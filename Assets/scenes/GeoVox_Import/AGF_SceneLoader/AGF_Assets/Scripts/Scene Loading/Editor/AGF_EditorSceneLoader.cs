using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class AGF_EditorSceneLoader : EditorWindow {
	
	private static AGF_LevelLoader m_LevelLoader;
	private enum SubState{
		LoadingScene, ExtractingWarehouseObjects, ExtractingVoxelObjects, FillingBar, ExtractingGeovoxObjects
	}
	private static SubState m_CurrentSubState = SubState.LoadingScene;
	
	private static AGF_LevelLoader.LoadSceneMode m_LoadSceneMode;
	
	private int m_CurrentWarehouseObject = 0;
	private int m_TotalNumberOfWarehouseObjects = 0;
	private int m_CurrentVoxelObject = 0;
	private Mesh voxelMesh;
	
	private int m_FrameDelay = 0;

	static Transform parentObject;
	public static List<Transform> prefabList;
	
	public static int LoadScene( string filePath, AGF_LevelLoader.LoadSceneMode mode = AGF_LevelLoader.LoadSceneMode.All, string overrideUserDataHomeFolder = "" ){
		if(GameObject.Find("Directional Light"))
			DestroyImmediate(GameObject.Find("Directional Light"));

		int returnCode = AGF_ReturnCode.Success;
		
		m_LoadSceneMode = mode;
		
		// Get the position of the integration window.
		Rect integrationWindowRect = AGF_IntegrationWindow.window.position;
		
		AGF_EditorSceneLoader window = GetWindowWithRect<AGF_EditorSceneLoader>( new Rect( integrationWindowRect.x + integrationWindowRect.width/2.0f, integrationWindowRect.y + integrationWindowRect.height/2.0f, 1.0f, 1.0f ) );
		window.position = new Rect( integrationWindowRect.x + integrationWindowRect.width/2.0f, integrationWindowRect.y + integrationWindowRect.height/2.0f, 1.0f, 1.0f );
		
		// Check that the integration prefab has all necessary components.
		if ( GameObject.Find ("AGF_LevelLoader") == null ||
		    GameObject.Find ("EventHandler") == null ||
		    GameObject.Find ("AGF_LevelLoader") == null ||
		    GameObject.Find ("AGF_AtmosphereManager") == null ||
		    GameObject.Find ("AGF_TileListManager") == null ||
		    GameObject.Find ("AGF_TerrainManager") == null ||
		    GameObject.Find ("AGF_GibManager") == null ||
		    GameObject.Find ("AGF_GridManager") == null ){
			window.CloseWindow();
			return AGF_ReturnCode.AGFIntegrationPrefabComponentMissing;
		}
		
		m_LevelLoader = GameObject.Find ("AGF_LevelLoader").GetComponent<AGF_LevelLoader>();
		
		// step 2: Init all gameobjects. (Most of these will simply call the Start() methods of the given object.)
		GameObject.Find ("EventHandler").GetComponent<EventHandler>().EditorInit();
		GameObject.Find ("AGF_LevelLoader").GetComponent<AGF_LevelLoader>().EditorInit();
		GameObject.Find ("AGF_AtmosphereManager").GetComponent<AGF_AtmosphereManager>().EditorInit();
		GameObject.Find ("AGF_TileListManager").GetComponent<AGF_TileListManager>().EditorInit();
		GameObject.Find ("AGF_TerrainManager").GetComponent<AGF_TerrainManager>().EditorInit();
		GameObject.Find ("AGF_GibManager").GetComponent<AGF_GibManager>().EditorInit();
		GameObject.Find ("AGF_GridManager").GetComponent<AGF_GridManager>().EditorInit();
		
		// step 3: loading the scene is now possible through the level loader.
		returnCode = m_LevelLoader.LoadScene( filePath, mode, overrideUserDataHomeFolder );
		if ( returnCode != AGF_ReturnCode.Success ){
			window.CloseWindow();	
			return returnCode;
		}
		
		// step 4: wait for an event callback to be received.
		m_CurrentSubState = SubState.LoadingScene;
		
		return returnCode;
	}

	public static int LoadGeovoxScene( string filePath, AGF_LevelLoader.LoadSceneMode mode = AGF_LevelLoader.LoadSceneMode.All, string overrideUserDataHomeFolder = "" ){

		int returnCode = AGF_ReturnCode.Success;
		
		m_LoadSceneMode = mode;
		
		// Get the position of the integration window.
		Rect integrationWindowRect = AGF_IntegrationWindow.window.position;
		
		AGF_EditorSceneLoader window = GetWindowWithRect<AGF_EditorSceneLoader>( new Rect( integrationWindowRect.x + integrationWindowRect.width/2.0f, integrationWindowRect.y + integrationWindowRect.height/2.0f, 1.0f, 1.0f ) );
		window.position = new Rect( integrationWindowRect.x + integrationWindowRect.width/2.0f, integrationWindowRect.y + integrationWindowRect.height/2.0f, 1.0f, 1.0f );
		
		// Check that the integration prefab has all necessary components.
		if ( GameObject.Find ("AGF_LevelLoader") == null ||
		    GameObject.Find ("EventHandler") == null ||
		    GameObject.Find ("AGF_LevelLoader") == null ||
		    GameObject.Find ("AGF_AtmosphereManager") == null ||
		    GameObject.Find ("AGF_TileListManager") == null ||
		    GameObject.Find ("AGF_TerrainManager") == null ||
		    GameObject.Find ("AGF_GibManager") == null ||
		    GameObject.Find ("AGF_GridManager") == null ){
			window.CloseWindow();
			return AGF_ReturnCode.AGFIntegrationPrefabComponentMissing;
		}
		
		m_LevelLoader = GameObject.Find ("AGF_LevelLoader").GetComponent<AGF_LevelLoader>();
		
		// step 2: Init all gameobjects. (Most of these will simply call the Start() methods of the given object.)
		GameObject.Find ("EventHandler").GetComponent<EventHandler>().EditorInit();
		GameObject.Find ("AGF_LevelLoader").GetComponent<AGF_LevelLoader>().EditorInit();
		GameObject.Find ("AGF_AtmosphereManager").GetComponent<AGF_AtmosphereManager>().EditorInit();
		GameObject.Find ("AGF_TileListManager").GetComponent<AGF_TileListManager>().EditorInit();
		GameObject.Find ("AGF_TerrainManager").GetComponent<AGF_TerrainManager>().EditorInit();
		GameObject.Find ("AGF_GibManager").GetComponent<AGF_GibManager>().EditorInit();
		GameObject.Find ("AGF_GridManager").GetComponent<AGF_GridManager>().EditorInit();
		
		// step 3: loading the scene is now possible through the level loader.
		returnCode = m_LevelLoader.LoadGeovoxScene( filePath, mode, overrideUserDataHomeFolder );
		if ( returnCode != AGF_ReturnCode.Success ){
			window.CloseWindow();	
			return returnCode;
		}
		
		// step 4: wait for an event callback to be received.
		m_CurrentSubState = SubState.LoadingScene;
		
		return returnCode;
	}
	
	private void Init(){
		Debug.Log ("Init has been called!");	
	}
	
	private void OnGUI(){
		float completionPercent = (float)m_CurrentWarehouseObject / (float)m_TotalNumberOfWarehouseObjects;
		if ( m_LoadSceneMode == AGF_LevelLoader.LoadSceneMode.CameraOnly ){
			EditorUtility.DisplayProgressBar( "Importing Camera From Scene.", "Loading Skybox from asset bundle...", completionPercent );
		} else if ( m_CurrentWarehouseObject == 0 ){
			EditorUtility.DisplayProgressBar( "Importing Scene.", "Instantiating objects...", completionPercent );
		}  else {
			EditorUtility.DisplayProgressBar( "Importing Scene.", "Extracting resources from asset bundle...", completionPercent );
		}
		
	}

	private void Update(){
		if ( m_CurrentSubState == SubState.LoadingScene ){
			if ( m_LevelLoader != null ){
				if ( m_LevelLoader.IsSceneLoaded() == false ){
					m_LevelLoader.EditorUpdate();
				} else if ( m_LevelLoader.IsSceneLoaded() == true ) {
					ApplyLoadedScene();
				} else if ( m_LevelLoader.IsSceneLoaded() == null ){
					// an error happened, close the window.
					CloseWindow();
				}
			} else {
				CloseWindow();	
			}
			
		} else if(m_CurrentSubState == SubState.ExtractingGeovoxObjects){
			if(m_TotalNumberOfWarehouseObjects == 0 && exportAttempts == 0){
			exportAttempts = 1;

				AGF_AssetBundleResourceExtractor.InitWarehouseExtractionLists ();
				AGF_AssetBundleResourceExtractor.InitWarehouseDirectories ();

				Transform voxelTerrain = FindObjectOfType<Geovox.VoxelTerrain>().transform;
				AGF_AssetBundleResourceExtractor.ExtractVoxelObject(voxelTerrain);
				voxelTerrain.parent = parentObject;


				m_TotalNumberOfWarehouseObjects = Mathf.Max(1, AGF_EditorSceneLoader.prefabList.Count);
				if(m_TotalNumberOfWarehouseObjects == 1)
					m_CurrentWarehouseObject = 1;
			}
			else if(m_CurrentWarehouseObject < m_TotalNumberOfWarehouseObjects){
				if (AGF_EditorSceneLoader.prefabList [m_CurrentWarehouseObject] != null) {
					AGF_AssetBundleResourceExtractor.ExtractWarehouseObject (AGF_EditorSceneLoader.prefabList [m_CurrentWarehouseObject]);
				}
				if(AGF_EditorSceneLoader.prefabList[m_CurrentWarehouseObject] != null)
					AGF_EditorSceneLoader.prefabList[m_CurrentWarehouseObject].SetParent(parentObject);

				m_CurrentWarehouseObject++;
				
				Repaint();
			}
			else{
				
//				AGF_TileListManager tileListManager = FindObjectOfType<AGF_TileListManager> ();
//				
//				int numberOfSets = tileListManager.GetNumberOfSets ();
//				for (int i = 0; i < numberOfSets; i++) {
//					int numberOfTiles = tileListManager.GetNumberOfTilesInSet (i);
//					
//					for (int j = 0; j < numberOfTiles; j++) {
//						Transform tile = tileListManager.GetTile (i, j);
//						if(tile.name != "Monster_Locator" && tile.name != "Start_Locator"){
////							Resources.UnloadAsset(tile);
//							DestroyImmediate (tileListManager.GetTile (i, j).gameObject, true);
//						}
//					}		
//				}
				
				
				// unload all asset bundles.
				m_LevelLoader.UnloadAllUserBundles();
				
				// unload all models.
				m_LevelLoader.ClearLoadList();

				DestroyImmediate(GameObject.Find("Geovox_Integration"));
					
				if(!FindObjectOfType<Light>()){
					Debug.Log("No light found. Adding...");
					GameObject newLight = new GameObject();
					newLight.name = "Directional Light";
					newLight.transform.rotation = Quaternion.Euler(new Vector3(45,45,0));
					Light m_Light = newLight.AddComponent<Light>();
					m_Light.type = LightType.Directional;
					m_Light.shadows = AGF_IntegrationWindow.lightShadows;
				}

				GameObject areaMarker = GameObject.Find("AreaMarker");
				if(areaMarker){

					DestroyImmediate(areaMarker);
				}

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				exportAttempts = 0;

				CloseWindow();
					
				foreach(TileProperties tile in FindObjectsOfType<TileProperties>()){
					Debug.LogError(tile.name);
					DestroyImmediate(tile.gameObject);
				}
				Debug.Log("Extraction Complete");
			}
		}
		else if ( m_CurrentSubState == SubState.FillingBar ){
			if ( m_FrameDelay > 0 ){
				m_FrameDelay--;	
			} else {
				CloseWindow();
			}
		}
	}

	int exportAttempts = 0;
	
	private void ApplyLoadedScene(){	
		// destroy all special tiles.
		Dictionary<int, Transform> placedTileList = GameObject.Find ("AGF_GridManager").GetComponent<AGF_GridManager>().GetPlacedTileList();
		List<int> specialTileList = new List<int>();
		foreach( KeyValuePair<int,Transform> pair in placedTileList ){
//			Debug.Log( pair.Value.GetComponent<TileProperties>().tileID );
			string[] split = pair.Value.GetComponent<TileProperties>().tileID.Split( new char[]{'/'} );
			if ( split[0] == "Special" && split[1] != "SphereLight" ){
//				Debug.Log("special found!");
				specialTileList.Add ( pair.Key );
			}
		}
		
		for ( int i = 0; i < specialTileList.Count; i++ ){
			Main.SmartDestroy( placedTileList[specialTileList[i]].gameObject );
			placedTileList.Remove( specialTileList[i] );
		}
		
		// run an update for the atmosphere to set everything.
		GameObject.Find ("AGF_AtmosphereManager").GetComponent<AGF_AtmosphereManager>().EditorUpdate();
		
		// create the source asset folder, if it does not already exist.
		string sourceAssetFolder = Main.TrimEndFromString( Application.dataPath, "Assets" ) + AGF_AssetBundleResourceExtractor.sourceFolder;
		if ( Directory.Exists( sourceAssetFolder ) == false ){
			Directory.CreateDirectory( sourceAssetFolder );
		}
		
		// create the scene folder, if it does not already exist. if it does exist, notify the user that a change will be made. (or if an overwrite should occur instead)
		string sceneName = m_LevelLoader.GetCurrentActiveSceneName();
		string sceneFolderName = sceneName;
		
		if ( Directory.Exists( sourceAssetFolder + "/" + sceneFolderName ) ){
			if ( m_LoadSceneMode == AGF_LevelLoader.LoadSceneMode.All ){
				bool result = EditorUtility.DisplayDialog( "Overwrite the " + sceneFolderName + " scene folder?", "An asset folder with the same name as the scene you wish to load already exists. " +
					"Should the folder be overwritten? This will delete all previous assets in that folder.", "Overwrite", "Rename new folder" );
				
				if ( result == true ){
					// The folder should be overwritten.
					Directory.Delete ( sourceAssetFolder + "/" + sceneFolderName, true );
				} else {
					// Rename the current folder.
					int count = 1;
					while ( Directory.Exists ( sourceAssetFolder + "/" + sceneFolderName ) ){
						sceneFolderName = sceneName + count.ToString();
						count++;
					}
				}
				
			} else if ( m_LoadSceneMode == AGF_LevelLoader.LoadSceneMode.CameraOnly ){
				bool result = EditorUtility.DisplayDialog( "Overwrite camera assets in the " + sceneFolderName + " scene folder?", "An asset folder with the same name as the scene you wish to load already exists. " +
					"Should the camera assets in the folder be overwritten? Other assets will remain untouched.", "Overwrite", "Rename new folder" );
				
				if ( result == false ){
					// Rename the current folder.
					int count = 1;
					while ( Directory.Exists ( sourceAssetFolder + "/" + sceneFolderName ) ){
						sceneFolderName = sceneName + count.ToString();
						count++;
					}	
				}
			}
			
		}
		Directory.CreateDirectory( sourceAssetFolder + "/" + sceneFolderName );
		
		// now the folder has been created, inform the resource extractor to save data into this folder.
		AGF_AssetBundleResourceExtractor.sceneFolder = sceneFolderName;
		
		if ( m_LoadSceneMode == AGF_LevelLoader.LoadSceneMode.All ){
			parentObject = new GameObject().transform;
			parentObject.name = sceneFolderName;
				
				Geovox.VoxelTerrain voxelTerrain = FindObjectOfType<Geovox.VoxelTerrain>();
				
				bool oldSaveMeshes = voxelTerrain.saveMeshes;
				bool oldHideChunks = voxelTerrain.hideChunks;
				bool oldHideWire = voxelTerrain.hideWire;
				bool oldMultiThreadEdit = voxelTerrain.multiThreadEdit;
				float oldLodDist = voxelTerrain.lodDistance;
				
				voxelTerrain.saveMeshes = true;
				voxelTerrain.hideChunks = false;
				voxelTerrain.hideWire = false;
				voxelTerrain.multiThreadEdit = false;
				voxelTerrain.generateLightmaps = voxelTerrain.guiBakeLightmap;
				voxelTerrain.lodDistance = 2000000000;
				
				//clearing and re-creating chunks
				voxelTerrain.chunks.Clear();
				voxelTerrain.Display(true);
				
				prefabList = new List<Transform>();
				//rebuilding //setting all chunk stages to "Force all"
				for (int i=0; i<voxelTerrain.chunks.array.Length; i++)
				{
					voxelTerrain.chunks.array[i].stage = Geovox.Chunk.Stage.forceAll;
					List<Transform> chunkTiles = voxelTerrain.chunks.array[i].Process();
					foreach(Transform tile in chunkTiles){
						if(tile != null){
							tile.parent = null;
							prefabList.Add(tile);
						}
						else{
					}
					}
				}
				
				voxelTerrain.saveMeshes = oldSaveMeshes;
				voxelTerrain.hideChunks = oldHideChunks;
				voxelTerrain.hideWire = oldHideWire;
				voxelTerrain.multiThreadEdit = oldMultiThreadEdit;
				voxelTerrain.generateLightmaps = false;
				voxelTerrain.lodDistance = oldLodDist;
				
				voxelTerrain.enabled = false;

				m_CurrentWarehouseObject = 0;

				m_CurrentSubState = SubState.ExtractingGeovoxObjects;

		} else if ( m_LoadSceneMode == AGF_LevelLoader.LoadSceneMode.CameraOnly ){
			AGF_AssetBundleResourceExtractor.ExtractSkybox();
			
			FinishSceneApplication();
		}
	}
	
	private void FinishSceneApplication(){
		if ( m_LoadSceneMode == AGF_LevelLoader.LoadSceneMode.All ){
			// copy the lighting gameobject.
			GameObject obj = Instantiate( GameObject.Find ("GameWorldLighting") ) as GameObject;
			obj.name = Main.TrimEndFromString( obj.name, "(Clone)" );
			
			m_CurrentSubState = SubState.FillingBar;
			m_FrameDelay = 200;
		} else {
			CloseWindow();
		}
		
		// unload all asset bundles.
		m_LevelLoader.UnloadAllUserBundles();
		
		// unload all models.
		m_LevelLoader.ClearLoadList();
		
		// once that's finished, delete the integration prefab.
		DestroyImmediate( GameObject.Find ("AGF_Integration") );
		
		// Display the new assets in the project tab.
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
	
	public void CloseWindow(){
		if (GameObject.Find ("Geovox_Integration")) {
			DestroyImmediate (GameObject.Find ("Geovox_Integration"));
		}


		EditorUtility.ClearProgressBar();
		Close ();
		AGF_IntegrationWindow.OnSceneLoaded();
	}
}
