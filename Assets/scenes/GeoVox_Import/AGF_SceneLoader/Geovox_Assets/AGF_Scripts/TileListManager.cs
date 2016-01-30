using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileListManager : MonoBehaviour {
	
	public Dictionary<string, Dictionary<string, Dictionary<string, Transform>>> m_TileList; 
	public List<string> m_CategoryOrder = new List<string>();
	private Dictionary<string, List<string>> m_TileOrder;
	private Dictionary<string, Dictionary<string, Dictionary<string, Transform>>> m_BundleList;
	public List<string> m_BundleOrder;
	private Dictionary<string, List<string>> m_TileBundleOrder;
	
	public enum SortMode{
		Category, Bundle,	
	}
	private SortMode m_CurrentSortMode = SortMode.Category;

	void Start(){
		m_TileOrder = new Dictionary<string, List<string>>();
		m_TileList = new Dictionary<string, Dictionary<string, Dictionary<string, Transform>>>();
		m_BundleList = new Dictionary<string, Dictionary<string, Dictionary<string, Transform>>>();
		m_TileBundleOrder = new Dictionary<string, List<string>>();
	}

	public void AddToList( string categoryName, string tileName, string bundleName, Transform tile ){
		Debug.LogError (categoryName + "+" + tileName + "+" + bundleName);
		if ( m_TileList.ContainsKey ( categoryName ) == false ){
			// create a new category in the main list.
			m_TileList[categoryName] = new Dictionary<string, Dictionary<string, Transform>>();
			
			// also place this new category in the ordering lists appropriately.
			m_CategoryOrder.Add ( categoryName );
			m_CategoryOrder.Sort();
			m_TileOrder.Add ( categoryName, new List<string>() );
		}
		
		// add the tile to the category. (if it already exists, replace it with this newer version)
		if ( m_TileList[categoryName].ContainsKey( tileName ) ){
			
			if ( m_TileList[categoryName][tileName].ContainsKey( bundleName ) == false ){
				m_TileOrder[categoryName].Add( tileName + "/" + bundleName );
				m_TileOrder[categoryName].Sort();
			}
			
			m_TileList[categoryName][tileName][bundleName] = tile;
			
		} else {
			m_TileList[categoryName][tileName] = new Dictionary<string, Transform>();
			m_TileList[categoryName][tileName][bundleName] = tile;
			
			m_TileOrder[categoryName].Add( tileName + "/" + bundleName );
			m_TileOrder[categoryName].Sort();
		}
		
		// Also add the tile reference to the bundle-sorted lists.
		if ( m_BundleList.ContainsKey( bundleName ) == false ){
			// create a new bundle in the list.
			m_BundleList[bundleName] = new Dictionary<string, Dictionary<string, Transform>>();
			
			// also place this new bundle in the ordering lists appropriately.
			m_BundleOrder.Add ( bundleName );
			m_BundleOrder.Sort ();
			m_TileBundleOrder.Add ( bundleName, new List<string>() );
		}
		
		// add the tile to the bundle. (if it already exists, replace it with this newer version)
		if ( m_BundleList[bundleName].ContainsKey( tileName )){
			
			if ( m_BundleList[bundleName][tileName].ContainsKey( categoryName ) == false ){
				m_TileBundleOrder[bundleName].Add ( tileName + "/" + categoryName );
				m_TileBundleOrder[bundleName].Sort ();
			}
			
			m_BundleList[bundleName][tileName][categoryName] = tile;
			
		} else {
			m_BundleList[bundleName][tileName] = new Dictionary<string, Transform>();
			m_BundleList[bundleName][tileName][categoryName] = tile;
			
			m_TileBundleOrder[bundleName].Add( tileName + "/" + categoryName );
			m_TileBundleOrder[bundleName].Sort();
		}
	}
	
	public void AddToList( string combinedName, Transform tile ){
		//		print ("AddToCategory: " + combinedName );
		string[] split = combinedName.Split (new char[]{'/'});
		string categoryName = split[0];
		string tileName = split[1];
		string bundleName = split[2];
		
		AddToList( categoryName, tileName, bundleName, tile );
	}
	
	public void AddToList( int setIndex, int tileIndex, Transform tile ){
		if ( m_CurrentSortMode == SortMode.Category ){
			string categoryName = m_CategoryOrder[setIndex];
			string combinedID = GetTileID( categoryName, tileIndex );
			AddToList( combinedID, tile );
		} else {
			string bundleName = m_BundleOrder[setIndex];
			string combinedID = GetTileID( bundleName, tileIndex );
			AddToList( combinedID, tile );
		}
	}
	
	public int GetNumberOfSets(){
		if ( m_CurrentSortMode == SortMode.Category ){
			return m_CategoryOrder.Count();	
		} else {
			return m_BundleOrder.Count();	
		}
	}
	
	
	// Get Number of Tiles in Set //
	public int GetNumberOfTilesInSet( string setName ){
		if ( m_CurrentSortMode == SortMode.Category ){
			if(m_TileList.ContainsKey(setName)){
				int tileCount = m_TileList[setName].Count();
				
				foreach ( KeyValuePair<string, Dictionary<string,Transform>> pair in m_TileList[setName] ){
					tileCount += (pair.Value.Count - 1);
				}
				
				return tileCount;
			}
			else{
				Debug.LogError(setName + " did not exist");
				return 0;
			}
		} else {
			string bundleName = setName;
			
			int tileCount = m_BundleList[bundleName].Count();
			foreach ( KeyValuePair<string, Dictionary<string,Transform>> pair in m_BundleList[bundleName] ){
				tileCount += (pair.Value.Count - 1);
			}
			
			return tileCount;
		}
	}
	
	public int GetNumberOfTilesInSet( int setIndex ){
		string setName = GetSetName(setIndex);
		return GetNumberOfTilesInSet( setName );
	}
	
	public int GetNumberOfTilesInSet( string categoryName, string bundleName ){
		if ( m_CurrentSortMode == SortMode.Category ){
			return GetNumberOfTilesInSet( categoryName );
		} else {
			return GetNumberOfTilesInSet( bundleName );
		}
	}
	
	// Get Tile //
	public Transform GetTile( string categoryName, string tileName, string bundleName ){
		if ( m_TileList.ContainsKey(categoryName) && m_TileList[categoryName].ContainsKey (tileName) && m_TileList[categoryName][tileName].ContainsKey(bundleName) ){
			return m_TileList[categoryName][tileName][bundleName];
		} else if(GameObject.Find(tileName)){
			return GameObject.Find (tileName).transform;
		}
		else
			return null;
		
	} 
	
	public Transform GetTile( string combinedName ){
		string[] split = combinedName.Split (new char[]{'/'});

		if (split.Length < 3)
			return null;

		string categoryName = split[0];
		string tileName = split[1];
		string bundleName = split[2];
		return GetTile( categoryName, tileName, bundleName );
	}
	
	public Transform GetTile( int setIndex, int tileIndex ){
		if ( m_CurrentSortMode == SortMode.Category ){
			string categoryName = m_CategoryOrder[setIndex];
			string combinedName = GetTileID ( categoryName, tileIndex );
			return GetTile ( combinedName );
		} else {
			string bundleName = m_BundleOrder[setIndex];
			string combinedName = GetTileID ( bundleName, tileIndex );
			return GetTile ( combinedName );
		}
	}
	
	public Transform GetTile( string setName, int tileIndex ){
		return GetTile ( GetTileID ( setName, tileIndex ) );
	}
	
	public string GetSetName( int setIndex ){
		if ( m_CurrentSortMode == SortMode.Category ){
			int categoryIndex = setIndex;
			return m_CategoryOrder[categoryIndex];	
		} else {
			int bundleIndex = setIndex;
			return m_BundleOrder[bundleIndex];	
		}
	}
	
	// Get Tile ID //
	public string GetTileID( string setName, int tileID ){
		if ( m_CurrentSortMode == SortMode.Category ){
			
			string tileBundleName = m_TileOrder[setName][tileID];
			string[] split = tileBundleName.Split( new char[]{'/'} );
			string tileName = split[0];
			string bundleName = split[1];
			return setName + "/" + tileName + "/" + bundleName;
		} else {
			string bundleName = setName;
			string tileCategoryName = m_TileBundleOrder[bundleName][tileID];
			string[] split = tileCategoryName.Split( new char[]{'/'} );
			string tileName = split[0];
			string categoryName = split[1];
			return categoryName + "/" + tileName + "/" + bundleName;
		}
	}
	
	public string GetTileID( string categoryName, string bundleName, int tileID ){
		if ( m_CurrentSortMode == SortMode.Category ){
			return GetTileID ( categoryName, tileID );
		} else {
			return GetTileID ( bundleName, tileID );
		}
	}
}
