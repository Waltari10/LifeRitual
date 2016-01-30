using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtmosphereManager : MonoBehaviour {
	
	public Shader skyboxShader;
	public Material test;
	
	// Sub-states, dictating the current appearance of the atmosphere.
	public enum SubState{
		Init, On, Off, Underwater, Build, Design,
	}
	private SubState m_CurrentSubState;
	
	// inspector values.
	public Camera skyboxCamera;
	//	public string[] skyboxResourcePaths;
	public Flare[] lightFlares;
	public Transform lightKit;
	
	private Dictionary<string,Dictionary<string,Texture2D>> loadedSkyboxes;
	
	// Properties Classes
	public abstract class AtmosphereProperties{
		public bool hasChanged;
		protected bool _active;
		protected bool _underwaterActive;
		protected AtmosphereManager m_AtmosphereManager;
		
		public bool active{
			get { return _active; }
			set { if ( value != _active ) {
					hasChanged = true;
					_active = value;
				}
			}
		}
		
		public bool underwaterActive{
			get { return _underwaterActive; }
			set { if ( value != _underwaterActive ) { hasChanged = true; _underwaterActive = value; } }
		}
		
		public AtmosphereProperties(){
			m_AtmosphereManager = FindObjectOfType<AtmosphereManager>();	
		}
		
		public abstract void UpdateOnMode();
		public abstract void UpdateOffMode();
		public abstract void UpdateUnderwaterMode();
	}
	
	public class SkyboxProperties : AtmosphereProperties{
		private Color _tint;
		//		private bool _autoRotate;
		//		private float _autoRotationSpeed;
		//		private float _rotation;
		private string _skyboxID;
		private Color _clearColor;
		private Color _offClearColor;
		private Texture2D cubemapTopFace;
		//Underwater Skybox Properties
		private Color _underwaterTint;
		private bool _underwaterAutoRotate;
		private float _underwaterAutoRotationSpeed;
		private float _underwaterRotation;
		private string _underwaterSkyboxID;
		private Color _underwaterClearColor;
		
		public Color tint{
			get { return _tint; }
			set { if ( value != _tint ) {hasChanged = true; _tint = value; } }
		}
		//		public float rotation{
		//			get { return _rotation; }
		//			set { if ( value != _rotation ) { _rotation = value; } }
		//		}
		//		public bool autoRotate{
		//			get { return _autoRotate; }
		//			set { if ( value != _autoRotate ) { _autoRotate = value; } }
		//		}
		//		public float autoRotationSpeed{
		//			get { return _autoRotationSpeed; }
		//			set { if ( value != _autoRotationSpeed ) { _autoRotationSpeed = value; } }
		//		}
		public string skyboxID{
			get { return _skyboxID; }
			set { if ( value != _skyboxID ) { 
					hasChanged = true; 
					_skyboxID = value; 
				} 
			}
		}
		public Color clearColor{
			get { return _clearColor; }
			set { if ( value != _clearColor ) { _clearColor = value; } }
		}
		public Color offClearColor{
			get { return _offClearColor; }
			set { if ( value != _offClearColor ) { _offClearColor = value; } }
		}
		
		//Underwater properties
		public Color underwaterTint{
			get { return _underwaterTint; }
			set { if ( value != _underwaterTint ) { _underwaterTint = value; } }
		}
		public bool underwaterAutoRotate{
			get { return _underwaterAutoRotate; }
			set { if ( value != _underwaterAutoRotate ) { _underwaterAutoRotate = value; } }
		}
		public float underwaterAutoRotationSpeed{
			get { return _underwaterAutoRotationSpeed; }
			set { if ( value != _underwaterAutoRotationSpeed ) { _underwaterAutoRotationSpeed = value; } }
		}
		public float underwaterRotation{
			get { return _underwaterRotation; }
			set { if ( value != _underwaterRotation ) { _underwaterRotation = value; } }
		}
		public string underwaterSkyboxID{
			get { return _underwaterSkyboxID; }
			set { if ( value != _underwaterSkyboxID ) { 
					hasChanged = true; 
					_underwaterSkyboxID = value; 
				} 
			}
		}
		public Color underwaterClearColor{
			get { return _underwaterClearColor; }
			set { if ( value != _underwaterClearColor ) { _underwaterClearColor = value; } }
		}

		
		public override void UpdateOnMode(){
		}
		
		public override void UpdateUnderwaterMode(){
		}
		
		public override void UpdateOffMode(){
		}
		
		public Texture2D GetCurrentSkyboxTexture(){
			string[] split = _skyboxID.Split(new char[]{'/'});
			if ( m_AtmosphereManager.loadedSkyboxes.ContainsKey( split[0] ) &&
			    m_AtmosphereManager.loadedSkyboxes[split[0]].ContainsKey( split[1] ) ){
				return m_AtmosphereManager.loadedSkyboxes[split[0]][split[1]];
			}
			else return null;
		}
		
		public Texture2D GetCurrentUnderwaterSkyboxTexture(){
			string[] split = _underwaterSkyboxID.Split(new char[]{'/'});
			if ( m_AtmosphereManager.loadedSkyboxes.ContainsKey( split[0] ) &&
			    m_AtmosphereManager.loadedSkyboxes[split[0]].ContainsKey( split[1] ) ){
				return m_AtmosphereManager.loadedSkyboxes[split[0]][split[1]];
			}
			else return null;
		}
	}
	
	public class HazeProperties : AtmosphereProperties{
		private bool _distanceMode;
		private float _density;
		private float _startDist;
		private float _height;
		private float _falloff;
		private Color _tint;
		
		private bool _underwaterDistanceMode;
		private float _underwaterDensity;
		private float _underwaterStartDist;
		private float _underwaterHeight;
		private float _underwaterFalloff;
		private Color _underwaterTint;

		public bool GetDistanceMode(){
			return _distanceMode;
		}
		public void SetDistanceMode(bool newDistanceMode){
			_distanceMode = newDistanceMode;
			hasChanged = true;
		}
		
		public float GetDensity(){
			return _density;
		}
		public void SetDensity(float newDensity){
			_density = newDensity;
			hasChanged = true;
		}
		
		
		
		public float startDist{
			get { return _startDist; }
			set { if ( value != _startDist ) { hasChanged = true; _startDist = value; } }
		}
		public float height{
			get { return _height; }
			set { if ( value != _height ) { hasChanged = true; _height = value; } }
		}
		public float falloff{
			get { return _falloff; }
			set { if ( value != _falloff ) { hasChanged = true; _falloff = value; } }
		}
		public Color tint{
			get { return _tint; }
			set { if ( value != _tint ) { hasChanged = true; _tint = value; } }
		}
		
		public bool underwaterDistanceMode{
			get { return _underwaterDistanceMode; }
			set { if ( value != _underwaterDistanceMode ) { hasChanged = true; _underwaterDistanceMode = value; } }
		}
		public float underwaterDensity{
			get { return _underwaterDensity; }
			set { if ( value != _underwaterDensity ) { hasChanged = true; _underwaterDensity = value; } }
		}
		public float underwaterStartDist{
			get { return _underwaterStartDist; }
			set { if ( value != _underwaterStartDist ) { hasChanged = true; _underwaterStartDist = value; } }
		}
		public float underwaterHeight{
			get { return _underwaterHeight; }
			set { if ( value != _underwaterHeight ) { hasChanged = true; _underwaterHeight = value; } }
		}
		public float underwaterFalloff{
			get { return _underwaterFalloff; }
			set { if ( value != _underwaterFalloff ) { hasChanged = true; _underwaterFalloff = value; } }
		}
		public Color underwaterTint{
			get { return _underwaterTint; }
			set { if ( value != _underwaterTint ) { hasChanged = true; _underwaterTint = value; } }
		}

		
		public HazeProperties() : base(){
		}
		
		public override void UpdateOnMode(){

		}
		
		public override void UpdateOffMode(){
		}
		
		public override void UpdateUnderwaterMode(){

		}
		
		
		
	}
	
	public class BasicFogProperties : AtmosphereProperties{
		private bool _linearMode;
		private float _startDist;
		private float _endDist;
		private float _density;
		private Color _tint;
		
		private bool _underwaterLinearMode;
		private float _underwaterStartDist;
		private float _underwaterEndDist;
		private float _underwaterDensity;
		private Color _underwaterTint;
		
		public bool linearMode{
			get { return _linearMode; }
			set { if ( value != _linearMode ) { hasChanged = true; _linearMode = value; } }
		}
		public float startDist{
			get { return _startDist; }
			set { if ( value != _startDist ) { hasChanged = true; _startDist = value; } }
		}
		public float endDist{
			get { return _endDist; }
			set { if ( value != _endDist ) { hasChanged = true; _endDist = value; } }
		}
		public float density{
			get { return _density; }
			set { if ( value != _density ) { hasChanged = true; _density = value; } }
		}
		public Color tint{
			get { return _tint; }
			set { if ( value != _tint ) { hasChanged = true; _tint = value; } }
		}
		
		public bool underwaterLinearMode{
			get { return _underwaterLinearMode; }
			set { if ( value != _underwaterLinearMode ) { hasChanged = true; _underwaterLinearMode = value; } }
		}
		public float underwaterStartDist{
			get { return _underwaterStartDist; }
			set { if ( value != _underwaterStartDist ) { hasChanged = true; _underwaterStartDist = value; } }
		}
		public float underwaterEndDist{
			get { return _underwaterEndDist; }
			set { if ( value != _underwaterEndDist ) { hasChanged = true; _underwaterEndDist = value; } }
		}
		public float underwaterDensity{
			get { return _underwaterDensity; }
			set { if ( value != _underwaterDensity ) { hasChanged = true; _underwaterDensity = value; } }
		}
		public Color underwaterTint{
			get { return _underwaterTint; }
			set { if ( value != _underwaterTint ) { hasChanged = true; _underwaterTint = value; } }
		}
		
		public override void UpdateOnMode(){
		}
		
		public override void UpdateOffMode(){
		}
		
		public override void UpdateUnderwaterMode(){
		}
	}
	
	public class AmbientLightProperties : AtmosphereProperties{
		private Color _tint;
		private Color _underwaterTint;
		
		public Color tint{
			get { return _tint; }
			set { if ( value != _tint ) { 
					hasChanged = true;
					_tint = value; } }
		}
		
		public Color underwaterTint{
			get { return _underwaterTint; }
			set { if ( value != _underwaterTint ) {
					hasChanged = true;
					_underwaterTint = value; } }
		}
		
		public override void UpdateOnMode(){

		}
		
		public override void UpdateOffMode(){

		}
		
		public override void UpdateUnderwaterMode(){
		}
	}
	
	public class BasicLightProperties : AtmosphereProperties{
		public Transform lightTransform;
		protected Light lightObject;
		
		protected Color _tint;
		private float _intensity;
		private float _yawAngle;
		private float _pitchAngle;
		
		private Color _underwaterTint;
		private float _underwaterIntensity;
		private float _underwaterYawAngle;
		private float _underwaterPitchAngle;
		
		private bool _offActive;
		private Color _offTint;
		private float _offIntensity;
		protected float _offYawAngle;
		protected float _offPitchAngle;
		
		public Color tint{
			get { return _tint; }
			set { if ( value != _tint ) { hasChanged = true; _tint = value; } }
		}
		public float intensity{
			get { return _intensity; }
			set { if ( value != _intensity ) { hasChanged = true; _intensity = value; } }
		}
		public float yawAngle{
			get { return _yawAngle; }
			set { if ( value != _yawAngle ) { hasChanged = true; _yawAngle = value; } }
		}
		public float pitchAngle{
			get { return _pitchAngle; }
			set { if ( value != _pitchAngle ) { hasChanged = true; _pitchAngle = value; } }
		}
		
		public Color underwaterTint{
			get { return _underwaterTint; }
			set { if ( value != _underwaterTint ) { hasChanged = true; _underwaterTint = value; } }
		}
		public float underwaterIntensity{
			get { return _underwaterIntensity; }
			set { if ( value != _underwaterIntensity ) { hasChanged = true; _underwaterIntensity = value; } }
		}
		public float underwaterYawAngle{
			get { return _underwaterYawAngle; }
			set { if ( value != _underwaterYawAngle ) { hasChanged = true; _underwaterYawAngle = value; } }
		}
		public float underwaterPitchAngle{
			get { return _underwaterPitchAngle; }
			set { if ( value != _underwaterPitchAngle ) { hasChanged = true; _underwaterPitchAngle = value; } }
		}
		
		public bool offActive{
			get { return _offActive; }
			set { if ( value != _offActive ) { hasChanged = true; _offActive = value; } }
		}
		public Color offTint{
			get { return _offTint; }
			set { if ( value != _offTint ) { hasChanged = true; _offTint = value; } }
		}
		public float offIntensity{
			get { return _offIntensity; }
			set { if ( value != _offIntensity ) { hasChanged = true; _offIntensity = value; } }
		}
		public float offYawAngle{
			get { return _offYawAngle; }
			set { if ( value != _offYawAngle ) { hasChanged = true; _offYawAngle = value; } }
		}
		public float offPitchAngle{
			get { return _offPitchAngle; }
			set { if ( value != _offPitchAngle ) { hasChanged = true; _offPitchAngle = value; } }
		}
		
		public BasicLightProperties( Transform newTransform ) : base(){
			lightTransform = newTransform;
			lightObject = lightTransform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Light>();
		}
		
		public override void UpdateOnMode(){
		}
		
		public override void UpdateOffMode(){
		}
		
		public override void UpdateUnderwaterMode(){
		}
	}
	
	public class KeyLightProperties : BasicLightProperties{
		private bool _shadowsActive;
		private float _shadowStrength;
		private bool _lightFlareActive;
		private int _flareID;
		
		private bool _underwaterShadowsActive;
		private float _underwaterShadowStrength;
		private bool _underwaterLightFlareActive;
		private int _underwaterFlareID;
		
		public bool shadowsActive{
			get { return _shadowsActive; }
			set { if ( value != _shadowsActive ) { hasChanged = true; _shadowsActive = value; } }
		}
		public float shadowStrength{
			get { return _shadowStrength; }
			set { if ( value != _shadowStrength ) { hasChanged = true; _shadowStrength = value; } }
		}
		public bool lightFlareActive{
			get { return _lightFlareActive; }
			set { if ( value != _lightFlareActive ) { hasChanged = true; _lightFlareActive = value; } }
		}
		public int flareID{
			get { return _flareID; }
			set { if ( value != _flareID ) { hasChanged = true; _flareID = value; } }
		}
		
		public bool underwaterShadowsActive{
			get { return _underwaterShadowsActive; }
			set { if ( value != _underwaterShadowsActive ) { hasChanged = true; _underwaterShadowsActive = value; } }
		}
		public float underwaterShadowStrength{
			get { return _underwaterShadowStrength; }
			set { if ( value != _underwaterShadowStrength ) { hasChanged = true; _underwaterShadowStrength = value; } }
		}
		public bool underwaterLightFlareActive{
			get { return _underwaterLightFlareActive; }
			set { if ( value != _underwaterLightFlareActive ) { hasChanged = true; _underwaterLightFlareActive = value; } }
		}
		public int underwaterFlareID{
			get { return _underwaterFlareID; }
			set { if ( value != _underwaterFlareID ) { hasChanged = true; _underwaterFlareID = value; } }
		}

		
		public KeyLightProperties( Transform newTransform ) : base( newTransform ){
		}
		
		public override void UpdateOnMode(){
		}
		
		public override void UpdateOffMode(){
		}
		
		public override void UpdateUnderwaterMode(){
		}
	}
	
	// Property class objects
	private SkyboxProperties m_CurrentSkybox;
	private HazeProperties m_CurrentHaze;
	private BasicFogProperties m_CurrentBasicFog;
	private AtmosphereProperties[] m_AtmosphereProperties;
	
	// scene object references
	private Transform m_CurrentLightKit;
	
	// individual visibility flags
	private bool m_AtmosphereVisible = true;
	
	// global light rotation
	private float m_GlobalLightRotation;
	
	[System.NonSerialized] public bool underwaterAtmosphereEnabled;
	
	private void Start(){
		// initialize all of the property trackers, and add them to the array.
		m_CurrentSkybox = new SkyboxProperties();
		m_CurrentHaze = new HazeProperties();
		m_CurrentBasicFog = new BasicFogProperties();
		
		m_AtmosphereProperties = new AtmosphereProperties[]{ m_CurrentSkybox, m_CurrentHaze, m_CurrentBasicFog};

		
		// initialize the texture list.
		loadedSkyboxes = new Dictionary<string, Dictionary<string, Texture2D>>();
		
		// setup the default atmosphere configuration.
		SetupDefaultConfiguration();

		// set the default state to 'on'.
		m_CurrentSubState = SubState.Init;
		this.ChangeState( SubState.On ); // this process will trigger all "hasChanged" flags to be set to true.
	}
	
	public void Reset(){
		m_AtmosphereVisible	= true;
	}
	
	private void SetupDefaultConfiguration(){
		// When the atmosphere is off, these values will be applied to the lighting, etc.
		
		m_CurrentSkybox.offClearColor = new Color(76.0f/255.0f, 83.0f/255.0f, 81.0f/255.0f, 255.0f/255.0f);

	}
	
	public bool GetUnderwaterAtmosEnabled(){
		return underwaterAtmosphereEnabled;
	}
	
	public void SetUnderwaterAtmosEnabled(bool newAtmosEnabled){
		underwaterAtmosphereEnabled = newAtmosEnabled;
	}
	
	// skybox
	public SkyboxProperties GetCurrentSkybox(){
		return m_CurrentSkybox;	
	}
	
	// fog
	public HazeProperties GetCurrentHaze(){
		return m_CurrentHaze;
	}
	
	public BasicFogProperties GetCurrentBasicFog(){
		return m_CurrentBasicFog;	
	}
	
	public bool IsAtmosphereActive(){
		return m_AtmosphereVisible;
	}
	
	public void SetAtmosphereActive( bool newActive ){
		if ( m_AtmosphereVisible != newActive ){
			for ( int i = 0; i < m_AtmosphereProperties.Length; i++ ){
				m_AtmosphereProperties[i].hasChanged = true;	
			}
		}
		m_AtmosphereVisible = newActive;
	}
	
	public void ChangeState( SubState newState ){
		if ( m_CurrentSubState != newState ){
			m_CurrentSubState = newState;
			for ( int i = 0; i < m_AtmosphereProperties.Length; i++ ){
				m_AtmosphereProperties[i].hasChanged = true;	
			}
		}
	}
	
	public void UpdateCurrentCharacterCubemap(){

	}
	
	public void AddTextureToList( Texture2D previewTexture, string bundleName ){
		// determine if this texture is a colormap or normalmap.
		if ( loadedSkyboxes.ContainsKey( bundleName ) == false ){
			loadedSkyboxes.Add ( bundleName, new Dictionary<string,Texture2D>() );	
		}
		string name = previewTexture.name;
//		print ("Add Texture to List: " + previewTexture.name);
		loadedSkyboxes[bundleName].Add ( name, previewTexture );	
	}
	
	public void UnloadTexturesFromBundle( string bundleName ){
		if ( loadedSkyboxes.ContainsKey( bundleName ) ){
			loadedSkyboxes.Remove ( bundleName );
			
			// if the skybox is currently in use, change back to a default skybox.
			if ( m_CurrentSkybox.skyboxID.Split( new char[]{'/'} )[0] == bundleName ){
				string cubemap = (new List<string>(loadedSkyboxes["Default_s.unity3d"].Keys))[0];
				m_CurrentSkybox.skyboxID = "Default_s.unity3d/" + cubemap;
			}
			if ( m_CurrentSkybox.underwaterSkyboxID.Split( new char[]{'/'} )[0] == bundleName ){
				string cubemap = (new List<string>(loadedSkyboxes["Default_s.unity3d"].Keys))[0];
				m_CurrentSkybox.underwaterSkyboxID = "Default_s.unity3d/" + cubemap;
			}
			
		}
	}
	
	public Dictionary<string,Texture2D> GetLoadedSkyboxTextures(){
		Dictionary<string,Texture2D> result = new Dictionary<string,Texture2D>();
		foreach ( KeyValuePair<string,Dictionary<string,Texture2D>> bundle in loadedSkyboxes ){
			foreach( KeyValuePair<string,Texture2D> texture in bundle.Value ){
				result.Add ( bundle.Key + "/" + texture.Key, texture.Value );	
			}
		} 
		return result;	
	}




	

	
	[RPC]
	public void SetHazeDistanceMode(bool newDistanceMode){
		m_CurrentHaze.SetDistanceMode (newDistanceMode);
	}
	
	[RPC]
	public void SetHazeDensity(float newDensity){
		m_CurrentHaze.SetDensity(newDensity);
	}
}
