//using UnityEngine;
//using System.Collections;
//
//public class TileProperties : MonoBehaviour {
//	
//	public string tileID = "";
//	public Texture2D tileTexture;
//
//	void Start(){
//		if(GetComponent<Rigidbody>())
//			Destroy(GetComponent<Rigidbody>());
//
//		if (!gameObject.GetComponent<BoxCollider> ()) {
//			MeshCollider col = GetComponent<MeshCollider> ();
//			if (!col)
//				col = gameObject.AddComponent<MeshCollider> ();
//
//			col.convex = true;
//		}
//	}
//}
