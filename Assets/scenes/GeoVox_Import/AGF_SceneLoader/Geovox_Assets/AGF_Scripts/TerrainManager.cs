using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class TerrainManager : MonoBehaviour {
	
	private Dictionary<string,Dictionary<string,Texture2D>> m_LoadedColormaps;
	private Dictionary<string,Dictionary<string,Texture2D>> m_LoadedNormals;
	private Dictionary<string,Dictionary<string,Texture2D>> m_LoadedVegetation;

	void Start(){
		
		m_LoadedColormaps = new Dictionary<string, Dictionary<string, Texture2D>>();
		m_LoadedNormals = new Dictionary<string, Dictionary<string, Texture2D>>();
		m_LoadedVegetation = new Dictionary<string, Dictionary<string, Texture2D>>();
	}
	
	public void AddTextureToList( Texture2D newTex, string bundleName ){
		// determine if this texture is a colormap or normalmap.
		if ( newTex.name.Contains("_c") || newTex.name.Contains("_Diffuse") ){
			if ( m_LoadedColormaps.ContainsKey( bundleName ) == false ){
				m_LoadedColormaps.Add ( bundleName, new Dictionary<string,Texture2D>() );	
			}
			string name = newTex.name.Replace( "_c", "" );
			name = name.Replace( "_Diffuse", "" );
			m_LoadedColormaps[bundleName].Add ( name, newTex );	
			
		} else if ( newTex.name.Contains("_n") || newTex.name.Contains("_Normal") ){
			if ( m_LoadedNormals.ContainsKey( bundleName ) == false ){
				m_LoadedNormals.Add ( bundleName, new Dictionary<string,Texture2D>() );	
			}
			string name = newTex.name.Replace( "_n", "" );
			name = name.Replace( "_Normal", "" );
			m_LoadedNormals[bundleName].Add ( name, newTex );		
		}
	}
	
	public void AddVegetationTextureToList( Texture2D newTex, string bundleName ){
		// determine if this texture is a colormap or normalmap.
		string name = "";
		if ( newTex.name.Contains("_c") || newTex.name.Contains("_Diffuse") ){
			if ( m_LoadedVegetation.ContainsKey( bundleName ) == false ){
				m_LoadedVegetation.Add ( bundleName, new Dictionary<string,Texture2D>() );	
			}
			name = newTex.name.Replace( "_c", "" );
			name = name.Replace( "_Diffuse", "" );
			
		} else if ( newTex.name.Contains("_n") || newTex.name.Contains("_Normal") ){
			if ( m_LoadedVegetation.ContainsKey( bundleName ) == false ){
				m_LoadedVegetation.Add ( bundleName, new Dictionary<string,Texture2D>() );	
			}
			name = newTex.name.Replace( "_n", "" );
			name = name.Replace( "_Normal", "" );	
		}
		
		m_LoadedVegetation[bundleName].Add ( name, newTex );	
	}
	
	public Dictionary<string,Texture2D> GetLoadedColormaps(){
		Dictionary<string,Texture2D> result = new Dictionary<string,Texture2D>();

		foreach ( KeyValuePair<string,Dictionary<string,Texture2D>> bundle in m_LoadedColormaps ){
			foreach( KeyValuePair<string,Texture2D> texture in bundle.Value ){
				result.Add ( bundle.Key + "/" + texture.Key, texture.Value );	
			}
		} 
		return result;	
	}
	
	public Dictionary<string,Texture2D> GetLoadedNormalmaps(){
		Dictionary<string,Texture2D> result = new Dictionary<string,Texture2D>();
		foreach ( KeyValuePair<string,Dictionary<string,Texture2D>> bundle in m_LoadedNormals ){
			foreach( KeyValuePair<string,Texture2D> texture in bundle.Value ){
				result.Add ( bundle.Key + "/" + texture.Key, texture.Value );	
			}
		} 
		return result;		
	}
	
	public Dictionary<string,Texture2D> GetLoadedVegetationTextures(){
		Dictionary<string,Texture2D> result = new Dictionary<string,Texture2D>();
		foreach ( KeyValuePair<string,Dictionary<string,Texture2D>> bundle in m_LoadedVegetation ){
			foreach( KeyValuePair<string,Texture2D> texture in bundle.Value ){
				result.Add ( bundle.Key + "/" + texture.Key, texture.Value );	
			}
		} 
		return result;	
	}
}
