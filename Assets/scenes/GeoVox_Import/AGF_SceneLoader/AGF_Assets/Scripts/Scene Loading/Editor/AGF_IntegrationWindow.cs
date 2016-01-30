using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class AGF_IntegrationWindow : EditorWindow {

	public enum IntegrationType{
		Geovox, AGF
	}
	[System.NonSerialized] public IntegrationType integrationType;
	
	public static AGF_IntegrationWindow window;
	private enum WindowState{
		ModeSelect, SceneImport, RuntimeLoader, ConfigureCamera,
		Help, HelpCamera, SetDirectory, GeovoxImport,
		LoadingScene, LoadingGeovoxScene, ErrorMissingAssetBundle, ErrorVersionNumber, Settings
	}
	private static WindowState m_CurrentWindowState = WindowState.ModeSelect;
	private static WindowState m_PrevWindowState = WindowState.ModeSelect;
	private static bool prevRunInBackground;
	[HideInInspector] public static string sourceDirectory = "";

	public static LightShadows lightShadows;
	public static bool terrainShadows = true;
	public static bool grassShadows = true;
	public static bool prefabShadows = true;
	public static bool terrainSpecular = true;
	
	[MenuItem( "Window/Geovox Scene Loader" )] 
	static void OpenGeovoxIntegrationWindow(){
		Resolution res = Screen.currentResolution;
		
		UpdateCurrentSourceDirectory();
		
		if ( window == null ){
			window = ScriptableObject.CreateInstance<AGF_IntegrationWindow>();
			window.ShowUtility();
			
			window.position = new Rect( res.width * 0.25f, res.height * 0.25f, 500.0f, 200 );
			window.title = "Geovox Scene Loader";
			window.maxSize = new Vector2( window.position.width, window.position.height );
			window.minSize = new Vector2( window.position.width, window.position.height );
			window.integrationType = IntegrationType.Geovox;
			
			m_CurrentWindowState = WindowState.ModeSelect;
			
		} else {
			window.Focus();
		}

		lightShadows = (LightShadows)PlayerPrefs.GetInt (Main.gameName + "ShadowType", 2);
		terrainShadows = PlayerPrefs.GetInt (Main.gameName + "TerrainShadowType", 1) == 1;
		grassShadows = PlayerPrefs.GetInt (Main.gameName + "GrassShadowType", 1) == 1;
		prefabShadows = PlayerPrefs.GetInt (Main.gameName + "PrefabShadowType", 1) == 1;
		terrainSpecular = PlayerPrefs.GetInt (Main.gameName + "TerrainSpecular", 1) == 1;
		
		prevRunInBackground = Application.runInBackground;
		Application.runInBackground = true;
	}
	
	private void OnDestroy(){
		Application.runInBackground = prevRunInBackground;	
	}
	
	private void OnGUI(){
		float currentHeight = 10.0f;
		Rect thisRect = window.position;

		string integrationPrefix = "";
		if(integrationType == IntegrationType.AGF){
			integrationPrefix = "AGF";
		}
		else if(integrationType == IntegrationType.Geovox){
			integrationPrefix = "Geovox";
		}

		if ( sourceDirectory == "" ){
			UpdateCurrentSourceDirectory();	
		}
			
		if (m_CurrentWindowState == WindowState.ModeSelect) {

			// Title Text
			DisplayCenteredText (ref currentHeight, 15, integrationPrefix + " Scene Loader");
			
			// Help Box
			float helpBoxSideMargin = 20.0f, helpBoxHeight = 20.0f;
			EditorGUI.HelpBox (new Rect (helpBoxSideMargin, currentHeight, thisRect.width - (helpBoxSideMargin * 2.0f), helpBoxHeight), "Choose which task you would like to perform.", MessageType.Info);
			currentHeight += 30.0f;
			
			float buttonWidth = 400.0f, buttonHeight = 20.0f;

			if (integrationType == IntegrationType.AGF) {
				//Warning label
				GUI.Label (new Rect (20, currentHeight, thisRect.width - 40, 60), "TURN OFF DIRECTX3D11\nTHE AGF TERRAIN SHADER DOES NOT WORK WITH " +
					"DX11 INSIDE UNITY\nFILE>BUILD SETTINGS>PLAYER SETTINGS>USE DIRECTX3D11\nOFF OR UNCHECKED.");
				currentHeight += 60;
			}
			
			// Buttons
			if (GUI.Button (new Rect (thisRect.width / 2.0f - buttonWidth / 2.0f, currentHeight, buttonWidth, buttonHeight), "Select default directory")) {
				m_CurrentWindowState = WindowState.SetDirectory;
			}
			currentHeight += 30.0f;
			
			if (GUI.Button (new Rect (thisRect.width / 2.0f - buttonWidth / 2.0f, currentHeight, buttonWidth, buttonHeight), "Import Geovox Scene")){
				if (!GameObject.Find ("Geovox_Integration")) {
					string path = "Assets/AGF_SceneLoader/AGF_Assets/Prefabs/Geovox_Integration.prefab";
					GameObject integrationPrefab = (GameObject)MonoBehaviour.Instantiate ((GameObject)AssetDatabase.LoadAssetAtPath (path, typeof(GameObject)));
					integrationPrefab.name = "Geovox_Integration";
					Debug.Log ("Integration prefab created");
				}
				if (GameObject.Find ("Geovox_Integration")) {
					int returnCode = LoadGeovoxScene ();
					if (returnCode != AGF_ReturnCode.Success) {
						Debug.LogError(returnCode);
						HandleLoadSceneError (returnCode);
					}
				} else {
					Debug.LogError ("Failed to create integration prefab from Assets/AGF_SceneLoader/AGF_Assets/Prefabs/Geovox_Integration.prefab");
				}
			}
			currentHeight += 30.0f;
			
			if (GUI.Button (new Rect (thisRect.width / 2.0f - buttonWidth / 2.0f, currentHeight, buttonWidth, buttonHeight), "Import Settings")) {
				m_CurrentWindowState = WindowState.Settings;
				m_PrevWindowState = WindowState.ModeSelect;
			}
			currentHeight += 30.0f;
			
			buttonWidth = 70.0f;
			buttonHeight = 20.0f;
			if (GUI.Button (new Rect (thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight), "Help")) {
				m_CurrentWindowState = WindowState.Help;
				m_PrevWindowState = WindowState.ModeSelect;
			}

//			if(GUI.Button(new Rect(20, thisRect.height - 40, 20, 20), "+")){
//				foreach(Transform chunk in FindObjectsOfType<Transform>()){
//					if(chunk.name == "HiResChunk"){
//						if(!AGF_IntegrationWindow.terrainSpecular){
//							chunk.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Standard");
//						}
//						else{
//							chunk.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Standard (Specular setup)");
//						}
//					}
//				}
//			}
//			
//			if(GUI.Button(new Rect(50, thisRect.height - 40, 20, 20), "+")){
//				foreach(Transform chunk in FindObjectsOfType<Transform>()){
//					if(chunk.name == "HiResChunk"){
//						if(!AGF_IntegrationWindow.terrainSpecular){
//							chunk.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Voxel/Standard");
//						}
//						else{
//							chunk.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Voxel/Standard (Specular setup)");
//						}
//					}
//				}
//			}
			
		} else if (m_CurrentWindowState == WindowState.Help) {
			// Title Text
			DisplayCenteredText (ref currentHeight, 15, "Help");
			
			float buttonWidth = 400.0f, buttonHeight = 20.0f;
			if (GUI.Button (new Rect (thisRect.width / 2.0f - buttonWidth / 2.0f, currentHeight, buttonWidth, buttonHeight), "My Unity Scene does not have a camera. What should I do?")) {
				m_CurrentWindowState = WindowState.HelpCamera;
			}
			currentHeight += 30.0f;
			
			buttonWidth = 70.0f;
			buttonHeight = 20.0f;
			if (GUI.Button (new Rect (thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight), "Back")) {
				m_CurrentWindowState = m_PrevWindowState;
			}
			
		} else if (m_CurrentWindowState == WindowState.Settings) {
			// Title Text
			DisplayCenteredText (ref currentHeight, 15, "Settings");
			
			float buttonWidth = 400.0f, buttonHeight = 20.0f;

			GUI.Label(new Rect(30, currentHeight, 200, 20), "Shadow type:");
			lightShadows = (LightShadows)EditorGUI.EnumPopup(new Rect(thisRect.width - 230, currentHeight, 200, 20), lightShadows);
			currentHeight += 20;
			
			GUI.Label(new Rect(30, currentHeight, 200, 20), "Terrain Shadows:");
			terrainShadows = EditorGUI.Toggle(new Rect(thisRect.width - 50, currentHeight, 20, 20), terrainShadows);
			currentHeight += 20;
			
			GUI.Label(new Rect(30, currentHeight, 200, 20), "Grass Shadows:");
			grassShadows = EditorGUI.Toggle(new Rect(thisRect.width - 50, currentHeight, 20, 20), grassShadows);
			currentHeight += 20;
			
			GUI.Label(new Rect(30, currentHeight, 200, 20), "Prefab Shadows:");
			prefabShadows = EditorGUI.Toggle(new Rect(thisRect.width - 50, currentHeight, 20, 20), prefabShadows);
			currentHeight += 20;
			
			GUI.Label(new Rect(30, currentHeight, 200, 20), "Use Terrain Specular:");
			terrainSpecular = EditorGUI.Toggle(new Rect(thisRect.width - 50, currentHeight, 20, 20), terrainSpecular);
			currentHeight += 20;
			
			buttonWidth = 70.0f;
			buttonHeight = 20.0f;
			if (GUI.Button (new Rect (thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight), "Back")) {
				PlayerPrefs.SetInt(Main.gameName + "ShadowType", (int)lightShadows);
				PlayerPrefs.SetInt(Main.gameName + "TerrainShadowType", terrainShadows ? 1 : 0);
				PlayerPrefs.SetInt(Main.gameName + "GrassShadowType", grassShadows ? 1 : 0);
				PlayerPrefs.SetInt(Main.gameName + "PrefabShadowType", prefabShadows ? 1 : 0);
				PlayerPrefs.SetInt(Main.gameName + "TerrainSpecular", terrainSpecular ? 1 : 0);


				m_CurrentWindowState = m_PrevWindowState;
			}
		}
		else if ( m_CurrentWindowState == WindowState.HelpCamera ){
			DisplayCenteredText( ref currentHeight, 15, "Camera Information" );
			
			DisplayWordWrappedCenteredText( ref currentHeight, 11, 
				"Unity provides a quick method of inserting a basic camera and character controller into your scene. " +
				"In the Menu Bar, navigate to Assets > Import Package. From there, click on \"Character Controller\", and import. " +
				"Drag in the \"First Person Controller\" prefab. You will notice that the prefab includes a main camera as one of its children.\n\n" +
				"When importing an " + integrationPrefix + " scene, select this new camera. Your " + integrationPrefix + " scene will load, apply all the necessary filters and skybox textures to the camera, and be ready to go!", 450.0f, true );
			
			float buttonWidth = 70.0f, buttonHeight = 20.0f;
			if ( GUI.Button ( new Rect( thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight ), "Back" ) ){
				m_CurrentWindowState = WindowState.Help;
			}
			
		} else if ( m_CurrentWindowState == WindowState.ConfigureCamera ){
			DisplaySceneImport( ref currentHeight, "Import Camera Settings from "+integrationPrefix + " Scene", "Use this utility if you have added a camera to your Unity scene after already performing the " +integrationPrefix + " scene import process.", AGF_LevelLoader.LoadSceneMode.CameraOnly, integrationType );
			
			float buttonWidth = 70.0f, buttonHeight = 20.0f;
			if ( GUI.Button ( new Rect( thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight ), "Back" ) ){
				m_CurrentWindowState = WindowState.ModeSelect;
			}
			
			if ( GUI.Button ( new Rect( thisRect.width - 20.0f - buttonWidth * 2.0f - 10.0f, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight ), "Help" ) ){
				m_PrevWindowState = WindowState.ConfigureCamera;
				m_CurrentWindowState = WindowState.Help;
			}
			
		} else if ( m_CurrentWindowState == WindowState.SceneImport ){
			DisplaySceneImport( ref currentHeight, "Import "+integrationPrefix + " Scene", "This utility will load a target "+integrationPrefix + " scene into the current active Unity scene. Note that a camera must exist within this Unity scene in order for the process to complete. (See help for more info.)", 
			                   AGF_LevelLoader.LoadSceneMode.All, integrationType );
			
			float buttonWidth = 70.0f, buttonHeight = 20.0f;
			if ( GUI.Button ( new Rect( thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight ), "Back" ) ){
				m_CurrentWindowState = WindowState.ModeSelect;
			}
			
			if ( GUI.Button ( new Rect( thisRect.width - 20.0f - buttonWidth * 2.0f - 10.0f, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight ), "Help" ) ){
				m_PrevWindowState = WindowState.SceneImport;
				m_CurrentWindowState = WindowState.Help;
			}
			
		} else if ( m_CurrentWindowState == WindowState.SetDirectory ){
			if(!Directory.Exists(sourceDirectory)){
				sourceDirectory = Main.GetUserDataHomeFolder();
				System.IO.File.WriteAllText( Application.dataPath + "/SourceDirectory.txt", sourceDirectory );
			}

			// Title Text
			DisplayCenteredText( ref currentHeight, 15, "Set Source Directory" );
			
			currentHeight = 50.0f;
			
			DisplayWordWrappedCenteredText( ref currentHeight, 11, "Select the default directory to search for Geovox scenes.\n" +
				"If this is not set, it will default to the directory of this scene loader.", 450.0f );
			
			currentHeight += 30.0f;
			
			GUI.TextField ( new Rect( thisRect.width/2.0f - 400.0f/2.0f, currentHeight, 400.0f, 18.0f ), sourceDirectory );
			currentHeight += 20.0f;
			
			if ( GUI.Button ( new Rect( thisRect.width/2.0f - 400.0f/2.0f, currentHeight, 180.0f, 20.0f ), "Select Directory..." ) ){
				string newSourceDir = EditorUtility.OpenFolderPanel( "Set Source Directory", sourceDirectory, "" );
				if ( newSourceDir != "" ){
					sourceDirectory = newSourceDir;
					System.IO.File.WriteAllText( Application.dataPath + "/SourceDirectory.txt", sourceDirectory );
				}
			}
			
			if ( GUI.Button ( new Rect( thisRect.width/2.0f - 400.0f/2.0f + 220.0f, currentHeight, 180.0f, 20.0f ), "Use Default" ) ){
				sourceDirectory = Main.GetUserDataHomeFolder();
				System.IO.File.WriteAllText( Application.dataPath + "/SourceDirectory.txt", sourceDirectory );
			}
			
			float buttonWidth = 70.0f, buttonHeight = 20.0f;
			if ( GUI.Button ( new Rect( thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight ), "Back" ) ){
				m_CurrentWindowState = WindowState.ModeSelect;
			}
			
		} else if ( m_CurrentWindowState == WindowState.RuntimeLoader ){
			// Title Text
			DisplayCenteredText( ref currentHeight, 15, "Runtime Loader Setup" );
			
			DisplayCenteredText( ref currentHeight, 18, "Coming soon!", true );
			
			float buttonWidth = 70.0f, buttonHeight = 20.0f;
			if ( GUI.Button ( new Rect( thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight ), "Back" ) ){
				m_CurrentWindowState = WindowState.ModeSelect;
			}
			
		} else if ( m_CurrentWindowState == WindowState.LoadingScene ){
			// Title Text
			DisplayCenteredText( ref currentHeight, 15, "Loading Scene...", true );
			
		} else if ( m_CurrentWindowState == WindowState.ErrorMissingAssetBundle ){
			int prevLabelSize = GUI.skin.label.fontSize;
			bool prevWordWrap = GUI.skin.label.wordWrap;
			
			GUIContent errorText = new GUIContent("Error! An Asset Bundle was missing. "
				+ "Make sure all asset bundles required for loading the scene exist within: " 
				+ sourceDirectory + "/Asset Packs/ " 
				+ "(Refer to the console for more information.) ");
			
			GUI.skin.label.fontSize = 15;
			GUI.skin.label.wordWrap = true;
			float height = GUI.skin.label.CalcHeight( errorText, 400.0f );
			GUI.Label ( new Rect( thisRect.width/2.0f - 400.0f/2.0f, thisRect.height/2.0f - height/2.0f, 400.0f, height ), errorText );
			
			GUI.skin.label.fontSize = prevLabelSize;
			GUI.skin.label.wordWrap = prevWordWrap;
			
			float buttonWidth = 70.0f, buttonHeight = 20.0f;
			if ( GUI.Button ( new Rect( thisRect.width - 20.0f - buttonWidth, thisRect.height - 10.0f - buttonHeight, buttonWidth, buttonHeight ), "Back" ) ){
				m_CurrentWindowState = WindowState.ModeSelect;
			}
			
		} else if ( m_CurrentWindowState == WindowState.ErrorVersionNumber ){
			
		}
	}
	
	private static void DisplaySceneImport( ref float currentHeight, string title, string description, AGF_LevelLoader.LoadSceneMode mode, IntegrationType currentIntegrationType ){

		Rect thisRect = window.position;
		Color prevColor = GUI.color;
		
		// Title Text
		DisplayCenteredText( ref currentHeight, 15, title );
		
		// Description Text
		DisplayWordWrappedCenteredText( ref currentHeight, 11, description, 400.0f );
		
		float buttonWidth = 200.0f, buttonHeight = 20.0f;
		
		// step 1: instantiate integration prefab.
		GameObject agfIntegration = GameObject.Find ("AGF_Integration");
		
		EditorGUI.HelpBox( new Rect( 20.0f, currentHeight, thisRect.width - (20.0f * 2.0f), 20.0f ), "Step 1: Instantiate the integration prefab.", MessageType.Info );
		GUI.Toggle( new Rect( thisRect.width - 40.0f, currentHeight, 20.0f, 20.0f ), agfIntegration != null, "" );
		currentHeight += 30.0f;
		
		if ( agfIntegration != null ){
			GUI.color = Color.grey;
		}
			
		if ( GUI.Button ( new Rect( thisRect.width/2.0f - buttonWidth/2.0f, currentHeight, buttonWidth, buttonHeight ), "Instantiate Prefab" ) ){
			if ( GUI.color != Color.grey ){
				string path = "Assets/AGF_SceneLoader/AGF_Assets/Prefabs/AGF_Integration.prefab";
				GameObject integrationPrefab = (GameObject)MonoBehaviour.Instantiate( (GameObject)AssetDatabase.LoadAssetAtPath( path, typeof(GameObject) ) );
				integrationPrefab.name = "AGF_Integration";
			}
		}
		GUI.color = prevColor;
		currentHeight += 32.0f;
		
		// step 2: Assign the camera link.
		GameObject cameraManager = GameObject.Find ("AGF_CameraManager");
		bool isCameraConfigured = (cameraManager != null && IsCameraConfigured( cameraManager.GetComponent<AGF_CameraManager>().mainCamera ) );
		
		EditorGUI.HelpBox( new Rect( 20.0f, currentHeight, thisRect.width - (20.0f * 2.0f), 20.0f ), "Step 2: Ensure that the main camera is properly configured.", MessageType.Info );
		GUI.Toggle( new Rect( thisRect.width - 40.0f, currentHeight, 20.0f, 20.0f ), isCameraConfigured, "" );
		currentHeight += 30.0f;
		
		DisplayCameraConfiguration( ref currentHeight );
		// step 3: Load the scene
		if ( mode == AGF_LevelLoader.LoadSceneMode.All && currentIntegrationType == IntegrationType.Geovox){
			EditorGUI.HelpBox( new Rect( 20.0f, currentHeight, thisRect.width - (20.0f * 2.0f), 20.0f ), "Step 3: Load the Geovox scene. The integration prefab will be destroyed.", MessageType.Info );
			currentHeight += 30.0f;

			if ( GUI.Button ( new Rect( thisRect.width/2.0f - buttonWidth/2.0f, currentHeight, buttonWidth, buttonHeight ), "Load Geovox Scene" ) ){
					int returnCode = LoadGeovoxScene();
					if ( returnCode != AGF_ReturnCode.Success ){
					Debug.LogError(returnCode);
						HandleLoadSceneError( returnCode );
					}
			}
		} else if ( mode == AGF_LevelLoader.LoadSceneMode.CameraOnly ){
			string integrationPrefix = currentIntegrationType.ToString();

			EditorGUI.HelpBox( new Rect( 20.0f, currentHeight, thisRect.width - (20.0f * 2.0f), 30.0f ), "Step 3: Load the camera settings from an " + integrationPrefix +" scene. The integration prefab will be destroyed.", MessageType.Info );
			currentHeight += 40.0f;
			
			if ( isCameraConfigured == false ){
				GUI.color = Color.grey;
			}
			if ( GUI.Button ( new Rect( thisRect.width/2.0f - 250.0f/2.0f, currentHeight, 250.0f, buttonHeight ), "Load Camera Settings from " + integrationPrefix +" Scene" ) ){
				if ( GUI.color != Color.grey ){
					int returnCode = LoadCameraFromScene();
					if ( returnCode != AGF_ReturnCode.Success ){
						Debug.LogError(returnCode);
						HandleLoadSceneError( returnCode );
					}
				}
			}
		}
		GUI.color = prevColor;
		currentHeight += 32.0f;
	}
	
	private static void DisplayCameraConfiguration( ref float currentHeight ){
		Rect thisRect = window.position;
		Color prevColor = GUI.color;
		
		GameObject cameraObj = GameObject.Find ("AGF_CameraManager");
		if ( cameraObj == null ){
			GUIContent errorText = new GUIContent("Prefab not instantiated.");
			Vector2 size = GUI.skin.GetStyle("Label").CalcSize( errorText );
			GUI.Label ( new Rect( thisRect.width/2.0f - size.x/2.0f, currentHeight, size.x, size.y ), errorText );
			currentHeight += size.y + 10.0f;
			return;
		}
		
		AGF_CameraManager cameraManager = cameraObj.GetComponent<AGF_CameraManager>();
		
		// first, assign the main camera prefab.
		bool cameraWasNull = !cameraManager.mainCamera;
		
		cameraManager.mainCamera = (Camera)EditorGUI.ObjectField(new Rect(thisRect.width/2.0f - 400.0f/2.0f, currentHeight, 400.0f, 15.0f), "Main Camera", cameraManager.mainCamera, typeof(Camera), true);
		if ( cameraWasNull && cameraManager.mainCamera != null ){
			// Set the prefab dirty if we detect a change.
			EditorUtility.SetDirty( cameraManager );	
		}
		currentHeight += 25.0f;
		
		// check if the camera is configured.
		bool isCameraConfigured = false;
		if ( cameraManager.mainCamera != null ){
			isCameraConfigured = IsCameraConfigured( cameraManager.mainCamera );
		}

		currentHeight += 30.0f;
	}
	
	private static void DisplayCenteredText( ref float currentHeight, int fontSize, string text, bool centerVertically = false ){
		Rect thisRect = window.position;
		
		int prevSize = GUI.skin.label.fontSize;
		GUIContent contentText = new GUIContent(text);
		GUI.skin.label.fontSize = fontSize;
		Vector2 size = GUI.skin.GetStyle("Label").CalcSize( contentText );
		
		if ( centerVertically ){
			GUI.Label ( new Rect( thisRect.width/2.0f - size.x/2.0f, thisRect.height/2.0f - size.y/2.0f, size.x, size.y ), contentText );
			currentHeight = thisRect.height/2.0f - size.y/2.0f + size.y + 10.0f;
		} else {
			GUI.Label ( new Rect( thisRect.width/2.0f - size.x/2.0f, currentHeight, size.x, size.y ), contentText );
			currentHeight += size.y + 10.0f;
		}
		
		
		GUI.skin.label.fontSize = prevSize;
	}
	
	private static void DisplayWordWrappedCenteredText( ref float currentHeight, int fontSize, string text, float width, bool centerVertically = false ){
		Rect thisRect = window.position;
		
		int prevSize = GUI.skin.label.fontSize;
		bool prevWordWrap = GUI.skin.label.wordWrap;
		GUIContent contentText = new GUIContent(text);
		GUI.skin.label.fontSize = fontSize;
		GUI.skin.label.wordWrap = true;
		float height = GUI.skin.GetStyle("Label").CalcHeight( contentText, width );
		Vector2 size = new Vector2(width, height);
		
		if ( centerVertically ){
			GUI.Label ( new Rect( thisRect.width/2.0f - size.x/2.0f, thisRect.height/2.0f - size.y/2.0f, size.x, size.y ), contentText );
			currentHeight = thisRect.height/2.0f - size.y/2.0f + size.y + 10.0f;
		} else {
			GUI.Label ( new Rect( thisRect.width/2.0f - size.x/2.0f, currentHeight, size.x, size.y ), contentText );
			currentHeight += size.y + 10.0f;
		}
		
		GUI.skin.label.fontSize = prevSize;
		GUI.skin.label.wordWrap = prevWordWrap;
	}
	
	private static int LoadGeovoxScene(){


		int returnCode = AGF_ReturnCode.Success;

		
		string filePath = EditorUtility.OpenFilePanel( "Load Geovox Scene", sourceDirectory, "agfv" );
		
		if ( filePath != "" ){
			Application.runInBackground = true;
			
			string[] pathSplit = filePath.Split ( new char[]{'/'} );
			string sceneName = pathSplit[pathSplit.Length-1];
			
			bool result = EditorUtility.DisplayDialog( "Load " + sceneName + "?", "This operation will instantiate all objects contained within" +
			                                          " the chosen Geovox Scene into the current Unity Scene. This process may take a long time. Are you sure you wish to do this?", "Load", "Cancel" );
			
			if ( result ){
				// we have the location of the scene, and confirmation from the user. begin the load process.
				returnCode = AGF_EditorSceneLoader.LoadGeovoxScene( filePath, AGF_LevelLoader.LoadSceneMode.All, sourceDirectory );
				m_CurrentWindowState = WindowState.LoadingGeovoxScene;
			}
		}
		
		return returnCode;
	}
	
	private static int LoadCameraFromScene(){
		int returnCode = AGF_ReturnCode.Success;
		
		string filePath = EditorUtility.OpenFilePanel( "Load AGF Scene", sourceDirectory, "agfs" );
		
		if ( filePath != "" ){
			string[] pathSplit = filePath.Split ( new char[]{'/'} );
			string sceneName = pathSplit[pathSplit.Length-1];
			
			bool result = EditorUtility.DisplayDialog( "Load the camera from " + sceneName + "?", "The camera settings from the chosen scene will be imported into your selected camera. " 
				+ "No other changes will be made during this process.", "Load", "Cancel" );
			
			if ( result ){
				// we have the location of the scene, and confirmation from the user. begin the load process.
				returnCode = AGF_EditorSceneLoader.LoadScene( filePath, AGF_LevelLoader.LoadSceneMode.CameraOnly, sourceDirectory );
				m_CurrentWindowState = WindowState.LoadingScene;
			}
		}
		
		return returnCode;
	}
	
	private static void HandleLoadSceneError( int errorCode ){
		if ( errorCode == AGF_ReturnCode.VersionMismatch ){
			Debug.LogError("Version numbers did not match!");
		} else if ( errorCode == AGF_ReturnCode.AssetBundleDoesNotExist ){
			m_CurrentWindowState = WindowState.ErrorMissingAssetBundle;
		} else if ( errorCode == AGF_ReturnCode.MissingModelFolder ){
			Debug.LogError("Model folder was missing!");	
		} else if ( errorCode == AGF_ReturnCode.AGFIntegrationPrefabComponentMissing ){
			Debug.LogError("The AGF_Integration prefab had an error. It has now been corrected. Please try re-importing the scene again.");
			if ( GameObject.Find ("AGF_Integration") ){
				DestroyImmediate( GameObject.Find ("AGF_Integration") );	
				m_CurrentWindowState = WindowState.SceneImport;
			}
		}
	}
	
	private static void DestroySourceAssetFolder(){
		string sourceFolder = Main.TrimEndFromString( Application.dataPath, "Assets" ) + AGF_AssetBundleResourceExtractor.sourceFolder;
		if ( Directory.Exists( sourceFolder ) == true ){
			Directory.Delete( sourceFolder, true );
		}
		
		AssetDatabase.Refresh();
	}
	
	// Set the window reference, so we can check if the window is open or not.
	private void OnEnable(){
		window = this;	
	}
	
	public static void OnSceneLoaded(){
		m_CurrentWindowState = WindowState.ModeSelect;
		
		if ( window != null ){
			window.Repaint();
		}
	}
	
	public static bool IsCameraConfigured( Camera cam ){
		if ( cam == null ) return false;
		return true;
	}	
	
	private static void DestroyAllComponents( Camera cam ){
	}
	
	private static void AddAllComponents( Camera cam ){
	}
	
	private static void UpdateCurrentSourceDirectory(){
		// check to see if a sourcedirectory.txt file exists. if not, use the default.
		if ( System.IO.File.Exists( Application.dataPath + "/SourceDirectory.txt" ) == false ){
			sourceDirectory = Main.GetUserDataHomeFolder();
		} else {
			sourceDirectory = System.IO.File.ReadAllText( Application.dataPath + "/SourceDirectory.txt" );	
		}
	}
}
