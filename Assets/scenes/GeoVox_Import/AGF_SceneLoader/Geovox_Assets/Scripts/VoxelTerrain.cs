
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Geovox
{
	public struct Coord
	{
		public int x; public int y; public int z; public byte dir; public byte extend;
		
		static readonly public int[] oppositeDirX = {0,0,1,-1,0,0};
		static readonly public int[] oppositeDirY = {1,-1,0,0,0,0};
		static readonly public int[] oppositeDirZ = {0,0,0,0,1,-1};
		
		public Coord (int x, int y, int z) {this.x=x; this.y=y; this.z=z; this.dir=0; this.extend=0;}
		public static Coord operator + (Coord a, Coord b) { return new Coord(a.x+b.x, a.y+b.y, a.z+b.z); }
		public static Coord operator - (Coord a, Coord b) { return new Coord(a.x-b.x, a.y-b.y, a.z-b.z); }
		public static Coord operator ++ (Coord a) { return new Coord(a.x+1, a.y+1, a.z+1); }
		
		public static bool operator > (Coord a, Coord b) { return (a.x>b.x || a.z>b.z); } //do not compare y's
		public static bool operator >= (Coord a, Coord b) { return (a.x>=b.x || a.z>=b.z); }
		public static bool operator < (Coord a, Coord b) { return (a.x<b.x || a.z<b.z); }
		public static bool operator <= (Coord a, Coord b) { return (a.x<=b.x || a.z<=b.z); }
		
		public Coord GetChunkCoord (Coord worldCoord, int chunkSize) //gets chunk coordinates using wholeterrain unit coord
		{
			return new Coord
				(
					worldCoord.x>=0 ? (int)(worldCoord.x/chunkSize) : (int)((worldCoord.x+1)/chunkSize)-1,
					0,
					worldCoord.z>=0 ? (int)(worldCoord.z/chunkSize) : (int)((worldCoord.z+1)/chunkSize)-1
					);
		}
		
		/*
		public int Extend
		{
			get { return y; } 
			set { y=value; }
		}*/
		
		public int BlockMagnitude2 { get { return Mathf.Abs(x)+Mathf.Abs(z); } }
		public int BlockMagnitude3 { get { return Mathf.Abs(x)+Mathf.Abs(y)+Mathf.Abs(z); } }
	}
	
	[System.Serializable]
	public class VoxelBlockType
	{
		#region class BlockType
		public string name;
		
		public bool filledTerrain = true; //just gui storage, using terrainExist instead. terrainExist is created upon display from 'fiiled' values
		
		public Color color = new Color(0.77f, 0.77f, 0.77f, 1f);
		public Texture texture; //aka mainTex
		public Texture bumpTexture; //aka bumpMap
		public Texture specGlossMap;
		public Color specular;
		public float tile = 0.25f;

		/*public bool differentTop;
		public Texture topTexture;
		public Texture topBumpTexture;*/
		public bool  grass = true;
		//public Texture grassTexture;
		public Color grassTint = Color.white;
		public float smooth = 1f; //not used
		
		//ambient
		public bool filledAmbient = true;
		
		//prefabs
		public bool filledPrefabs = false;
		public Transform[] prefabs = new Transform[0]; //new ObjectPool[0];
		public List<float> offset = new List<float>();
		public List<bool> alignToNormals = new List<bool>();
		public List<float> objectNormalArea = new List<float>();
		public List<bool> randomRotation = new List<bool>();
		public List<float> scaleRamp = new List<float>();
		public List<bool> collidersActive = new List<bool>();
		
		public VoxelBlockType (string n, bool f) { name = n; filledTerrain = f; filledAmbient = f; }
		#endregion
	}
	
	[System.Serializable]
	public class VoxelGrassType 
	{
		#region class GrassType
		public string name;
		public Mesh sourceMesh; //for editor only
		[System.NonSerialized] public MeshWrapper[] meshes;
		public Material material;
		
		public float height = 1;
		public float width = 1;
		public float elevation = 0;
		public float incline = 0.1f;
		public float random = 0.1f;
		public bool normalsFromTerrain = true;
		
		public VoxelGrassType (string n) { name = n; }
		#endregion
	}
	
	//[ExecuteInEditMode] //to rebuild on scene loading. Only OnEnable uses it
	public class VoxelTerrain : MonoBehaviour
	{

		public enum RebuildType {none, all, terrain, ambient, constructor, grass, prefab}; 
		
		public GeovoxData data;

		[System.NonSerialized] public Matrix2<Chunk> chunks = new Matrix2<Chunk>(0,0);
		[System.NonSerialized] public int[] chunkNumByDistance = new int[0]; //chunks ordered by distance from center. Always the same length as chunks.array
		
		public VoxelBlockType[] types;
		public VoxelGrassType[] grass;
		public int selected; //negative is for grass
		
		public bool[] terrainExist = new bool[0];
		public bool[] ambientExist = new bool[0];
		public bool[] prefabsExist = new bool[0];
		
		public int chunkSize = 30;
		public int terrainMargins = 2;

		
		public int brushSize = 0;
		public bool  brushSphere;
		
		public bool  playmodeEdit;
		
		public bool limited = true;
		
		public int chunksBuiltThisFrame = 0; //for statistics and farmesh
		public int chunksUnbuiltLeft = 0;

		#region vars Far
		public bool useFar = false;
		public Mesh farMesh;
		public float farSize;
		public Far far;
		#endregion
		
		public bool  ambient = true;
		public float ambientFade = 0.666f;
		public int ambientMargins = 5;
		//public int ambientSpread = 4;
		
		public Material terrainMaterial;
		
		#region vars GUI
		public bool guiBakeLightmap;
		public int guiEmptyColumnHeight = 0;
		public byte guiEmptyColumnType = 1;
		public int guiNewChunkSize = 30;
		public int guiAreaSize = 512;
		#endregion
	
		public bool generateLightmaps = false; //this should be always off exept baking with lightmaps
		public float lightmapPadding = 0.1f;
		public bool  saveMeshes = false;
		
		public float lodDistance = 50;
		public bool lodWithMaterial = true; //switch lod object using it's material, not renderer
		
		public float removeDistance = 120;
		public float generateDistance = 60;
		public bool gradualGenerate = true;
		public bool multiThreadEdit;

		public bool rtpCompatible = false;
		
		#if UNITY_EDITOR
		private System.Diagnostics.Stopwatch timerSinceSetBlock = new System.Diagnostics.Stopwatch(); //to save byte list
		#endif
		
		//update speed-ups
		[System.NonSerialized] public Ray oldAimRay; //public for vizualizer
		
		static public readonly int[] oppositeDirX = {0,0,1,-1,0,0};
		static public readonly int[] oppositeDirY = {1,-1,0,0,0,0};
		static public readonly int[] oppositeDirZ = {0,0,0,0,1,-1};
		
		#if UNITY_EDITOR
		private int mouseButton = 0; //for EditorUpdate, currently pressed mouse button
		#endif

		//debug
		public bool  hideChunks = true;
		public bool hideWire = true;
		public bool profile = false;
		public Visualizer visualizer;

		
		public bool chunksUnparented = false; //for saving scene without meshes. If chunks unparented they should be returned
		
		#region undo
		bool recordUndo = true;
		public List< Matrix2<GeovoxData.Column> > undoSteps = new List<Matrix2<GeovoxData.Column>>();

		#endregion

		VoxelInterface voxelInterface;
		
		void Start(){
			voxelInterface = FindObjectOfType<VoxelInterface> ();

			selected = 1;
			
			foreach (VoxelGrassType type in grass) {
				if(type.material){
					type.material = new Material(type.material);
				}
			}
		}		


	
		#region Display

			//public void Display () { Display(false); }
			public void Display (bool isEditor) //called every frame
			{			
				if (profile) Profiler.BeginSample ("Display");
				if (data == null) return;

				#region Loading compressed data
				if (data.areas == null) data.LoadFromByteList(data.compressed);
				#endregion
			
				#region Creating material if it has not been assigned
				if (terrainMaterial==null) terrainMaterial = new Material(Shader.Find("Voxel/Standard"));
				#endregion
			
				#region Returning unparented chunks
				if (chunksUnparented)
				{
					if (chunks.array != null) for (int i=0; i<chunks.array.Length; i++) chunks.array[i].transform.parent = transform;
					if (far != null) far.transform.parent = transform;
					chunksUnparented = false;
				}
				#endregion

				#region Rebuild: Clearing children if no terrain
				if (chunks.array == null || chunks.array.Length == 0)
					for(int i=transform.childCount-1; i>=0; i--) 
					{
						//clearing all meshes
						Chunk chunk = transform.GetChild(i).GetComponent<Chunk>();
						if (chunk != null)
						{
							if (chunk.hiFilter != null && chunk.hiFilter.sharedMesh != null) DestroyImmediate(chunk.hiFilter.sharedMesh);
							if (chunk.loFilter != null && chunk.loFilter.sharedMesh  != null) DestroyImmediate(chunk.loFilter.sharedMesh);
							if (chunk.grassFilter != null && chunk.grassFilter.sharedMesh != null)  DestroyImmediate(chunk.grassFilter.sharedMesh);
							//if (chunk.constructorFilter != null && chunk.constructorFilter.sharedMesh != null)  DestroyImmediate(chunk.constructorFilter.sharedMesh); //dconstructor
							//if (chunk.constructorCollider != null && chunk.constructorCollider.sharedMesh != null)  DestroyImmediate(chunk.constructorCollider.sharedMesh); //dconstructor
						}

						//destroing object. Whatever it is
						DestroyImmediate(transform.GetChild(i).gameObject);
					}
				#endregion

				#region Save Compressed Data after delay
				#if UNITY_EDITOR
				if (timerSinceSetBlock.ElapsedMilliseconds > 2000 && mouseButton == 0 && !UnityEditor.EditorApplication.isPlaying) 
				{
					data.compressed = data.SaveToByteList();
					UnityEditor.EditorUtility.SetDirty(data);
					timerSinceSetBlock.Stop();
					timerSinceSetBlock.Reset();
				}
				#endif
				#endregion
			
				#region Setting exist types
				if (terrainExist.Length != types.Length)
				{
					terrainExist = new bool[types.Length];
					ambientExist = new bool[types.Length];
					prefabsExist = new bool[types.Length];
					//dconstructor
					//constructorExist = new bool[types.Length]; 
					//terrainOrConstructorExist = new bool[types.Length];
				}
			
				for (int i=0; i<types.Length; i++)
				{
					terrainExist[i] = types[i].filledTerrain;
					ambientExist[i] = types[i].filledAmbient;
					prefabsExist[i] = types[i].filledPrefabs;
					//dconstructor
					//constructorExist[i] = types[i].filledConstructor;
					//terrainOrConstructorExist[i] = types[i].filledTerrain || types[i].filledConstructor;
				}
				#endregion
			
				#region Finding camera position

					Vector3 camPos;

					#if UNITY_EDITOR
					if (isEditor && UnityEditor.SceneView.lastActiveSceneView != null) 
						camPos = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
					else camPos = Camera.main.transform.position;
					#else
					camPos = Camera.main.transform.position;
					#endif
			
					camPos = transform.InverseTransformPoint(camPos);

				#endregion
			
				#region Preparing chunk matrix
					//all the chunks.array should be filled with chunks. There may be empty chunks (if they are out of build dist), but they should not be null
					//TODO: matrix changes too often on chunk seams. Make an offset.

					//finding build center and range (they differ on limited and infinite tarrains)
					float range = 0; Vector3 center = Vector3.zero;
			if (limited) { 
				GameObject areaMarker = GameObject.Find("AreaMarker");
				range = (guiAreaSize-chunkSize)/2f;

				if(!areaMarker || areaMarker.transform.position == Vector3.zero)
					center = new Vector3(range,range,range);
				else{
					center = areaMarker.transform.position;
					transform.position = new Vector3(-areaMarker.transform.position.x, 0, -areaMarker.transform.position.z);
				}
			}
			else { 
				range = removeDistance; 
				center = camPos; 
			}
					
					//looking if size changed
					bool matrixChanged = false;
					int newSize = Mathf.CeilToInt(range*2/chunkSize)+1;
					if (chunks.sizeX != newSize || chunks.sizeZ != newSize) matrixChanged = true;
			
					//looking if coordinates changed. If old center chunk != new center chunk
					int newOffsetX = Mathf.FloorToInt((center.x - range) / chunkSize);
					int newOffsetZ = Mathf.FloorToInt((center.z - range) / chunkSize);
					if (chunks.offsetX != newOffsetX || chunks.offsetZ != newOffsetZ) matrixChanged = true;

					//moving or resizing matrix
					if (matrixChanged)
					{
						//creating new chunks matrix
						Matrix2<Chunk> newChunks = new Matrix2<Chunk>(newSize, newSize);
						newChunks.offsetX = newOffsetX;
						newChunks.offsetZ = newOffsetZ;

						//taking all ready chunks
						for (int x=newChunks.offsetX; x<newChunks.offsetX+newChunks.sizeX; x++)
							for (int z=newChunks.offsetZ; z<newChunks.offsetZ+newChunks.sizeZ; z++)
								if (chunks.CheckInRange(x,z)) { newChunks[x,z] = chunks[x,z]; chunks[x,z] = null; }

						//creating list of unused chunks
						List<Chunk> unusedChunks = new List<Chunk>();
						for (int x=chunks.offsetX; x<chunks.offsetX+chunks.sizeX; x++)
							for (int z=chunks.offsetZ; z<chunks.offsetZ+chunks.sizeZ; z++)
								if (chunks[x,z] != null) unusedChunks.Add(chunks[x,z]);

						//filling empty holes with unused chunks (or creating new if needed - upon expanding range or rebuild)
						for (int x=newChunks.offsetX; x<newChunks.offsetX+newChunks.sizeX; x++)
							for (int z=newChunks.offsetZ; z<newChunks.offsetZ+newChunks.sizeZ; z++)
						{
							if (newChunks[x,z]==null)
							{
							//	if (unusedChunks.Count!=0) { newChunks[x,z] = unusedChunks[0]; unusedChunks.RemoveAt(0); }
							//	else 
								newChunks[x,z] = Chunk.CreateChunk(this);
								newChunks[x,z].Init(x,z);
							}
						}

						//removing unused chunks left
						for (int i=0; i<unusedChunks.Count; i++)
							DestroyImmediate(unusedChunks[i].gameObject);

						//saving array
						chunks = newChunks;
						
						//create an array of chunks ordered by distance
						if (chunkNumByDistance.Length != chunks.array.Length)
						{
							chunkNumByDistance = new int[chunks.array.Length];
							for (int i=0; i<chunkNumByDistance.Length; i++) chunkNumByDistance[i] = i;

							int[] distances = new int[chunks.array.Length];
							for (int z=0; z<chunks.sizeZ; z++)
								for (int x=0; x<chunks.sizeX; x++)
									distances[z*chunks.sizeX + x] = (x-chunks.sizeX/2)*(x-chunks.sizeX/2) + (z-chunks.sizeZ/2)*(z-chunks.sizeZ/2);
									//Mathf.Max( Mathf.Abs(x-chunks.sizeX/2), Mathf.Abs(z-chunks.sizeZ/2) );

							for (int i=0; i<distances.Length; i++) 
								for (int d=0; d<distances.Length-1; d++)
									if (distances[d] > distances[d+1])
									{
										int temp = distances[d+1];
										distances[d+1] = distances[d];
										distances[d] = temp;

										temp = chunkNumByDistance[d+1];
										chunkNumByDistance[d+1] = chunkNumByDistance[d];
										chunkNumByDistance[d] = temp;
									}
						}
					}
				#endregion



				#region Far mesh			
					if (useFar)
					{
						if (far == null) 
						{ 
							far = Far.Create(this);
							far.x = (int)camPos.x; far.z = (int)camPos.z;
							far.Build(); 
						}
						far.x = (int)camPos.x; far.z = (int)camPos.z;
						
						//rebuilding far on matrix change. Rebuild on chunk build is done in chunk via process
						if (matrixChanged) far.Build();
					}
					else if (far != null) Destroy(far.gameObject);
				#endregion
				
				#region Building terrain
				if (profile) Profiler.BeginSample ("Rebuild");
				
					chunksBuiltThisFrame = 0;
					for (int i=0; i<chunks.array.Length; i++)
					{
						Chunk chunk = chunks.array[chunkNumByDistance[i]];
						
						//skipping chunks that are out of build distance
						if (!limited && //in limited terrain all chunks should be built
							(chunk.offsetX + chunkSize < camPos.x - generateDistance ||
							chunk.offsetX > camPos.x + generateDistance ||
							chunk.offsetZ + chunkSize < camPos.z - generateDistance ||
							chunk.offsetZ > camPos.z + generateDistance))
								continue;
						
						//building forced chunks forced
						if (chunk.stage == Chunk.Stage.forceAll || chunk.stage == Chunk.Stage.forceAmbient)
							chunk.Process();

						//building gradual chunks gradually
						else if (chunk.stage != Chunk.Stage.complete) 
						{
							chunk.Process();
							if (gradualGenerate) break;
						}
					}
			
				if (profile) Profiler.EndSample();
				#endregion
			
				#region Calculating number of unbuilt chunks
				chunksUnbuiltLeft = 0;
				for (int i=0; i<chunks.array.Length; i++)
				{
					Chunk chunk = chunks.array[i];
						
					//skipping chunks that are out of build distance
					if (!limited && //in limited terrain all chunks should be built
						(chunk.offsetX + chunkSize < camPos.x - generateDistance ||
						chunk.offsetX > camPos.x + generateDistance ||
						chunk.offsetZ + chunkSize < camPos.z - generateDistance ||
						chunk.offsetZ > camPos.z + generateDistance))
							continue;
					
					if (chunk.stage != Chunk.Stage.complete)
						chunksUnbuiltLeft++;
				}
				//this should be in statistics
				//if (chunksBuiltThisFrame!=0 || chunksUnbuiltLeft!=0) Debug.Log(chunksBuiltThisFrame + " " + chunksUnbuiltLeft);
				#endregion
				
				#region Switching lods
				if (profile) Profiler.BeginSample ("Switching Lods");

					int minX = Mathf.FloorToInt(1f*(camPos.x-lodDistance)/chunkSize); int maxX = Mathf.FloorToInt(1f*(camPos.x+lodDistance)/chunkSize);
					int minZ = Mathf.FloorToInt(1f*(camPos.z-lodDistance)/chunkSize); int maxZ = Mathf.FloorToInt(1f*(camPos.z+lodDistance)/chunkSize);
					
					for (int x = chunks.offsetX; x < chunks.offsetX+chunks.sizeX; x++)
						for (int z = chunks.offsetZ; z < chunks.offsetZ+chunks.sizeZ; z++)
						{
							Chunk chunk = chunks[x,z];
							if (chunk == null) continue;

							if (x>=minX && x<=maxX && z>=minZ && z<=maxZ) chunk.SwitchLod(false);
							else chunk.SwitchLod(true);
						}
				
				if (profile) Profiler.EndSample();
				#endregion
				

				if (profile) Profiler.EndSample();
			}
		#endregion
		

		#region Edit	
			
			public struct AimData
			{
				public enum Type {none, face, obj, constructor};
				
				public bool hit;
				public Type type;
				public int x; public int y; public int z; public byte dir;
				public Chunk.Face face;
				public Collider collider;
				public int triIndex;
				public Chunk chunk;
			}
		
			public AimData GetCoordsByRay (Ray aimRay) 
			{
				AimData data = new AimData();
			
				RaycastHit raycastHitData; 
				if (!Physics.Raycast(aimRay, out raycastHitData) || //if was not hit
					!raycastHitData.collider.transform.IsChildOf(transform)) //if was hit in object that is not child of voxeland
						return data;
			
				data.collider = raycastHitData.collider;
			
				#region Terrain block
				if (raycastHitData.collider.gameObject.name == "LoResChunk")
				{
					data.chunk = raycastHitData.collider.transform.parent.GetComponent<Chunk>();
				
					if (data.chunk == null || data.chunk.faces==null || data.chunk.faces.Length==0) return data;
					if (data.chunk.stage != Chunk.Stage.complete) return data; //can hit only fully built chunks
				
					data.hit = true;
					data.type = AimData.Type.face;
					data.face = data.chunk.faces[ data.chunk.visibleFaceNums[raycastHitData.triangleIndex/2] ];
					data.x = data.face.x + data.chunk.coordX*chunkSize;
					data.y = data.face.y;
					data.z = data.face.z + data.chunk.coordZ*chunkSize;
					data.dir = data.face.dir;
				
					data.triIndex = raycastHitData.triangleIndex;
				
					return data;
				}
				#endregion
			
				#region Object block
				else
				{
					Transform parent = raycastHitData.collider.transform.parent;
					while (parent != null)
					{
						if (parent.name=="Chunk" && parent.IsChildOf(transform))
						{
							data.chunk = parent.GetComponent<Chunk>();
							break;
						}
						parent = parent.parent;
					}
				
					if (data.chunk == null) return data; //aiming other obj
				
					data.hit = true;
					data.type = AimData.Type.obj;
				
					Vector3 pos= raycastHitData.collider.transform.localPosition;
				
					data.x = (int)pos.x + data.chunk.offsetX; 
					data.y = (int)pos.y; 
					data.z = (int)pos.z + data.chunk.offsetZ;
	
					return data;
				}
				#endregion	
			}
		#endregion


		#region Get/Set Block
	
			public byte GetBlock (int x, int y, int z) { return data.GetBlock(x,y,z); }
		
			public void SetBlock (int x, int y, int z, byte type) { SetBlocks(x,y,z,type, 0, false, SetBlockMode.standard); } 
		
			public enum SetBlockMode {none, standard, replace, blur};
			
			public void SetBlocks (int x, int y, int z, byte type=0, int extend=0, bool spherify=false, SetBlockMode mode=SetBlockMode.none) //x,y,z are center, extend is half size (radius)
			{
				//registering undo
				if (recordUndo && mode != SetBlockMode.none)
				{
					undoSteps.Add( data.GetColumnMatrix(x-extend, z-extend, extend*2+1, extend*2+1) );
					//Debug.Log(undoSteps[undoSteps.Count-1].array.Length);
					if (undoSteps.Count > 32) { undoSteps.RemoveAt(0); undoSteps.RemoveAt(0); } //removing two steps...just to be sure
				}
			
				//starting timer
				#if UNITY_EDITOR
				timerSinceSetBlock.Reset();
				timerSinceSetBlock.Start();
				#endif
			
				//setting
				if (mode == SetBlockMode.standard || mode == SetBlockMode.replace)
				{
					//dconstructor
					//if (types[type].constructor != null) Scaffolding.Add(x,y,z);
					//else Scaffolding.Remove(x,y,z);			

					for (int xi = x-extend; xi<= x+extend; xi++)
						for (int yi = y-extend; yi<= y+extend; yi++) 
							for (int zi = z-extend; zi<= z+extend; zi++)
					{
						if (spherify && Mathf.Abs(Mathf.Pow(xi-x,2)) + Mathf.Abs(Mathf.Pow(yi-y,2)) + Mathf.Abs(Mathf.Pow(zi-z,2)) - 1 > extend*extend) continue;
						if (mode==SetBlockMode.replace && !types[ data.GetBlock(xi,yi,zi) ].filledTerrain) continue;
						if(yi > 2)
						data.SetBlock(xi, yi, zi, type);
					}
				}

				//blurring
				if (mode == SetBlockMode.blur) 
				{
					bool[] refExist = new bool[types.Length];
					for (int i=0; i<types.Length; i++) refExist[i] = types[i].filledTerrain;
					data.Blur(x,y,z, extend, spherify, refExist);
				}

				//mark areas as 'saved'
				for (int xi = x-extend; xi<= x+extend; xi++)
					for (int zi = z-extend; zi<= z+extend; zi++)
						data.areas[ data.GetAreaNum(xi,zi) ].save = true;

				//resetting progress
				
				//ambient
				for (int cx = Mathf.FloorToInt(1f*(x-extend-ambientMargins)/chunkSize); cx <= Mathf.FloorToInt(1f*(x+extend+ambientMargins)/chunkSize); cx++)
					for (int cz = Mathf.FloorToInt(1f*(z-extend-ambientMargins)/chunkSize); cz <= Mathf.FloorToInt(1f*(z+extend+ambientMargins)/chunkSize); cz++)
				{
					if (!chunks.CheckInRange(cx,cz)) continue;
					Chunk chunk = chunks[cx,cz];
					if (chunk!=null) chunk.stage = Chunk.Stage.forceAmbient;
					
				}

				//terrain
				for (int cx = Mathf.FloorToInt(1f*(x-extend-terrainMargins)/chunkSize); cx <= Mathf.FloorToInt(1f*(x+extend+terrainMargins)/chunkSize); cx++)
					for (int cz = Mathf.FloorToInt(1f*(z-extend-terrainMargins)/chunkSize); cz <= Mathf.FloorToInt(1f*(z+extend+terrainMargins)/chunkSize); cz++)
				{
					if (!chunks.CheckInRange(cx,cz)) continue;
					Chunk chunk = chunks[cx,cz];
					if (chunk!=null) chunk.stage = Chunk.Stage.forceAll;
					//and then calculating grass, prefabs, etc. Just because grass should change with land geometry
				}
			}
		
			public void SetGrass (int x, int z, byte type, int extend, bool spherify)
			{
				#if UNITY_EDITOR
				timerSinceSetBlock.Reset();
				timerSinceSetBlock.Start();
				#endif
			
				int minX = x-extend; int minZ = z-extend;
				int maxX = x+extend; int maxZ = z+extend;
			
				for (int xi = minX; xi<=maxX; xi++)
					for (int zi = minZ; zi<=maxZ; zi++)
						if (!spherify || Mathf.Abs(Mathf.Pow(xi-x,2)) + Mathf.Abs(Mathf.Pow(zi-z,2)) - 1 <= extend*extend) 
							data.SetGrass(xi, zi, type);
						
				Chunk chunk = chunks[Mathf.FloorToInt(1f*x/chunkSize), Mathf.FloorToInt(1f*z/chunkSize)];
				if (chunk!=null) chunk.stage = Chunk.Stage.forceAll;
			}
		
			public void ResetProgress (int x, int z, int extend) { SetBlocks(x,0,z,0,extend,false,SetBlockMode.none); }

		#endregion



		#region random array
		public static float[] random = {0.15f, 0.5625f, 0.69375f, 0.6125f, 0.76875f, 0.6625f, 0.4f, 0.99375f, 0.15625f, 0.68125f, 0.375f, 0.96875f, 
			0.025f, 0.63125f, 0.975f, 0.575f, 0.8875f, 0.69375f, 0.5875f, 0.775f, 0.7375f, 0.55f, 0.14375f, 0.025f, 0.69375f, 0.75f, 0.26875f, 0.35625f, 0.9f, 
			0.20625f, 0.33125f, 0.55f, 0.71875f, 0.40625f, 0.0625f, 0.98125f, 0.74375f, 0.38125f, 0.00625f, 0.8125f, 0.48125f, 0.1125f, 0.5f, 0.98125f, 0.26875f, 
			0.425f, 0.34375f, 0.7625f, 0.35625f, 0.94375f, 0.04375f, 0.29375f, 0.63125f, 0.2375f, 0.6625f, 0.85625f, 0.91875f, 0.78125f, 0.05625f, 0.33125f, 
			0.4125f, 0.6625f, 0.85f, 0.6f, 0.1125f, 0.89375f, 0.5875f, 0.25f, 0.79375f, 0.15625f, 0.5875f, 0.50625f, 0.4125f, 0.8f, 0.025f, 0.58125f, 0.5625f, 
			0.9625f, 0.475f, 0.4375f, 0.7375f, 0.60625f, 0.73125f, 0.2375f, 0.0375f, 0.50625f, 0.0125f, 0.2875f, 0.28125f, 0.65625f, 0.39375f, 0.925f, 0.83125f, 
			0.35625f, 0.49375f, 0.05625f, 0.29375f, 0.55f, 0.39375f, 0.86875f, 0.4875f, 0.3625f, 0.74375f, 0.30625f, 0.5875f, 0.625f, 0.96875f, 0.975f, 0.0375f, 
			0.3125f, 0.71875f, 0.98125f, 0.06875f, 0.66875f, 0.25625f, 0.3375f, 0.4f, 0.89375f, 0.04375f, 0.60625f, 0.0375f, 0.1625f, 0.25f, 0.13125f, 0.6375f, 
			0.04375f, 0.35625f, 0.8625f, 0.41875f, 0.78125f, 0.60625f, 0.85f, 0.10625f, 0.7125f, 0.54375f, 0.5375f, 0.85f, 0.3f, 0.3f, 0.96875f, 0.5375f, 0.925f, 
			0.89375f, 0.8625f, 0.64375f, 0.94375f, 0.225f, 0.93125f, 0.85625f, 0.275f, 0.0625f, 0.1625f, 0.63125f, 0.975f, 0.60625f, 0.5625f, 0.025f, 0.825f, 
			0.975f, 0.0125f, 0.875f, 0.8125f, 0.2875f, 0.83125f, 0.675f, 0.25625f, 0.53125f, 0.6125f, 0.25f, 0.4125f, 0.96875f, 0.94375f, 0.6125f, 0.54375f, 
			0.275f, 0.63125f, 0.94375f, 0.0625f, 0.2f, 0.9125f, 0.26875f, 0.9f, 0.84375f, 0.4375f, 0.73125f, 0.55625f, 0.375f, 0.4f, 0.925f, 0.13125f, 0.36875f, 
			0.6375f, 0.25625f, 0.725f, 0.49375f, 0.00625f, 0.89375f, 0.4f, 0.725f, 0.03125f, 0.0875f, 0.36875f, 0.88125f, 0.90625f, 0.55625f, 0.29375f, 0.625f, 
			0.9625f, 0.6875f, 0.55f, 0.70625f, 0.8125f, 0.21875f, 0.31875f, 0.63125f, 0.975f, 0.2375f, 0.25f, 0.4625f, 0.4375f, 0.96875f, 0.75625f, 0.0375f, 
			0.925f, 0.4f, 0.43125f, 0.2375f, 0.2125f, 0.21875f, 0.89375f, 0.6f, 0.40625f, 0.8625f, 0.41875f, 0.925f, 0.425f, 0.35f, 0.63125f, 0.15625f, 0.59375f, 
			0.8f, 0.7375f, 0.25f, 0.73125f, 0.01875f, 0.4625f, 0.14375f, 0.425f, 0.2625f, 0.24375f, 0.74375f, 0.05f, 0.50625f, 0.81875f, 0.9375f, 0.175f, 0.34375f, 
			0.70625f, 0.5125f, 0.8375f, 0.8125f, 0.81875f, 0.10625f, 0.43125f, 0.28125f, 0.66875f, 0.95625f, 0.68125f, 0.725f, 0.35f, 0.58125f, 0.38125f, 0.4375f, 
			0.8125f, 0.29375f, 0.3625f, 0.69375f, 0.5875f, 0.5625f, 0.53125f, 0.13125f, 0.55625f, 0.03125f, 0.2125f, 0.63125f, 0.79375f, 0.475f, 0.79375f, 0.3375f, 
			0.14375f, 0.25f, 0.875f, 0.55f, 0.60625f, 0.66875f, 0.9f, 0.29375f, 0.0625f, 0.78125f, 0.45f, 0.1567f, 0.6f, 0.03125f, 0.25f, 0.975f, 0.8f, 0.28125f, 
			0.0125f, 0.96875f, 0.29375f, 0.54375f, 0.13125f, 0.39375f, 0.04375f, 0.15625f, 0.49375f, 0.11875f, 0.5f, 0.5625f, 0.24375f, 0.86875f, 0.2125f, 0.39375f, 
			0.925f, 0.03125f, 0.03125f, 0.15625f, 0.68125f, 0.05625f, 0.3875f, 0.2f, 0.09375f, 0.65f, 0.8625f, 0.15625f, 0.24375f, 0.9875f, 0.58125f, 0.6125f, 
			0.56875f, 0.3f, 0.5375f, 0.19375f, 0.15625f, 0.81875f, 0.425f, 0.88125f, 0.04375f, 0.50625f, 0.0875f, 0.5625f, 0.11875f, 0.625f, 0.2f, 0.825f, 0.15625f, 
			0.5875f, 0.225f, 0.94375f, 0.76875f, 0.95625f, 0.275f, 0.975f, 0.56875f, 0.23125f, 0.975f, 0.04375f, 0.4875f, 0.28125f, 0.875f, 0.6625f, 0.5f, 0.8875f, 
			0.60625f, 0.125f, 0.6375f, 0.3875f, 0.25f, 0.94375f, 0.6375f, 0f, 0.06875f, 0.3f, 0.0875f, 0.53125f, 0.25f, 0.39375f, 0.36875f, 0.23125f, 0.64375f, 
			0.8375f, 0.21875f, 0.56875f, 0.2f, 0.81875f, 0.41875f, 0.44375f, 0.09375f, 0.26875f, 0.19375f, 0.9f, 0.23125f, 0.78125f, 0.89375f, 0.60625f, 0.1375f, 
			0.375f, 0.675f, 0.3125f, 0.91875f, 0.05f, 0.60625f, 0.56875f, 0.2875f, 0.075f, 0.925f, 0.725f, 0.3375f, 0.94375f, 0.91875f, 0.74375f, 0.09375f, 0.1375f, 
			0.2125f, 0.71875f, 0.48125f, 0.43125f, 0.80625f, 0.6f, 0.7625f, 0.8625f, 0.675f, 0.0625f, 0.0625f, 0.26875f, 0.4f, 0.075f, 0.10625f, 0.4875f, 0.49375f, 
			0.0125f, 0.66875f, 0.9f, 0.9f, 0.875f, 0.93125f, 0.6625f, 0.475f, 0.54375f, 0.29375f, 0.2625f, 0.775f, 0.58125f, 0.73125f, 0.61875f, 0.999f, 0.4375f, 
			0.2875f, 0.48125f, 0.45f, 0.71875f, 0.83125f, 0.3125f, 0.34375f, 0.24375f, 0.625f, 0.41875f, 0.30625f, 0.6375f, 0.84375f, 0.44375f, 0.39375f, 0.13125f, 
			0.5875f, 0.11875f, 0.05f, 0.30625f, 0.53125f, 0.29375f, 0.7f, 0.575f, 0.79375f, 0.1125f, 0.5f, 0.94375f, 0.0375f, 0.4875f, 0.93125f, 0.9875f, 0.61875f,
			0.59375f, 0.7f, 0.55f, 0.5f, 0.61875f, 0.70625f, 0.13125f, 0.999f, 0.475f, 0.45f, 0.3375f, 0.00625f, 0.725f, 0.78125f, 0.525f, 0.6f, 0.3375f, 0.1875f, 
			0.975f, 0.975f, 0.6875f, 0.65f, 0.6f, 0.3375f, 0.2375f, 0.24375f, 0.7375f, 0.875f, 0.4625f, 0.45625f, 0.81875f, 0.31875f, 0.375f, 0.475f, 0.56875f, 
			0.33125f, 0.2f, 0.3375f, 0.61875f, 0.125f, 0.65625f, 0.29375f, 0.95625f, 0.6125f, 0.24375f, 0.1375f, 0.28125f, 0.48125f, 0.1375f, 0.0375f, 0.7f, 
			0.475f, 0f, 0.4625f, 0.6375f, 0.3f, 0.3375f, 0.75625f, 0.6f, 0.4f, 0.7f, 0.10625f, 0.63125f, 0.09375f, 0.39375f, 0.29375f, 0.95625f, 0.20625f, 
			0.98125f, 0.95f, 0.81875f, 0.7875f, 0.15f, 0.875f, 0.96875f, 0.00625f, 0.41875f, 0.0625f, 0.7f, 0.85f, 0.075f, 0.925f, 0.025f, 0.6f, 0.9f, 0.36875f, 
			0.90625f, 0.88125f, 0.08125f, 0.36875f, 0.88125f, 0.15625f, 0.84375f, 0.53125f, 0.33125f, 0.48125f, 0.19375f, 0.275f, 0.6125f, 0.36875f, 0.325f, 
			0.30625f, 0.075f, 0.6f, 0.1125f, 0.825f, 0.5875f, 0.90625f, 0.1125f, 0.23125f, 0.28125f, 0.15625f, 0.675f, 0.74375f, 0.23125f, 0.25625f, 0.48125f, 
			0.7125f, 0.85625f, 0.10625f, 0.1125f, 0.025f, 0.64375f, 0.00625f, 0.9875f, 0.45f, 0.05f, 0.85625f, 0.275f, 0.11875f, 0.15625f, 0.4f, 0.09375f, 
			0.2875f, 0.95625f, 0.0375f, 0.675f, 0.38125f, 0.75f, 0.325f, 0.66875f, 0.4125f, 0.7875f, 0.71875f, 0.425f, 0f, 0.28125f, 0.70625f, 0.33125f, 0.9f, 
			0.15f, 0.9f, 0.03125f, 0.25625f, 0.675f, 0.16875f, 0.71875f, 0.675f, 0.13125f, 0.45f, 0.1125f, 0.1375f, 0.95f, 0.21875f, 0.60625f, 0.875f, 0.20625f, 
			0.4125f, 0.925f, 0.375f, 0.09375f, 0.675f, 0.9f, 0.48125f, 0.14375f, 0.675f, 0.7875f, 0.8125f, 0.8875f, 0.53125f, 0.5f, 0.45f, 0.6875f, 0.9625f, 
			0.43125f, 0.175f, 0.4875f, 0.19375f, 0.06875f, 0.13125f, 0.1375f, 0.325f, 0.1625f, 0.21875f, 0.03125f, 0.187f, 0.63125f, 0.1125f, 0.85625f, 0.88125f, 
			0.91875f, 0.2125f, 0.99375f, 0.675f, 0.0625f, 0.39375f, 0.55f, 0.00625f, 0.18125f, 0.85625f, 0.45625f, 0.68125f, 0.50625f, 0.98125f, 0.55625f, 0.8875f, 
			0.85f, 0.50625f, 0.29375f, 0.99375f, 0.5625f, 0.83125f, 0.85625f, 0.39375f, 0.83125f, 0.85625f, 0.1875f, 0.65625f, 0.43125f, 0.16875f, 0.0875f, 
			0.43125f, 0.69375f, 0.91875f, 0.51875f, 0.3875f, 0.9375f, 0.20625f, 0.3875f, 0.56875f, 0.025f, 0.4125f, 0.1375f, 0.4375f, 0.34375f, 0.1875f, 0.075f, 
			0.0875f, 0.79375f, 0.55f, 0.05f, 0.8f, 0.1625f, 0.86875f, 0.275f, 0.425f, 0.5f, 0.125f, 0.925f, 0.8375f, 0.73125f, 0.29375f, 0.35f, 0.91875f, 0.24375f, 
			0.0875f, 0.475f, 0.24375f, 0.70625f, 0.06875f, 0.26875f, 0.025f, 0.675f, 0.375f, 0.7875f, 0.425f, 0.4625f, 0.88125f, 0.80625f, 0.84375f, 0.7625f, 
			0.20625f, 0.4125f, 0.20625f, 0.3375f, 0.43125f, 0.63125f, 0f, 0.525f, 0.7125f, 0.44375f, 0.94375f, 0.7125f, 0.8375f, 0.2875f, 0.01875f, 0.55f, 0.93125f, 
			0.39375f, 0f, 0.0625f, 0.23125f, 0.01875f, 0.90625f, 0.4625f, 0.2125f, 0.98125f, 0.625f, 0.79375f, 0.9375f, 0.4875f, 0.2375f, 0.2f, 0.99375f, 0.7625f, 
			0.15f, 0.99375f, 0.83125f, 0.35625f, 0.5875f, 0.21875f, 0.88125f, 0.69375f, 0.61875f, 0.04375f, 0.21875f, 0.775f, 0.26875f, 0.54375f, 0.23125f, 0.5125f, 
			0.7625f, 0.16875f, 0.01875f, 0.69375f, 0.575f, 0.7f, 0.99375f, 0.00625f, 0.34375f, 0.725f, 0.35f, 0.1890f, 0.24375f, 0.53125f, 0.975f, 0.19375f, 0.90625f, 
			0.5125f, 0.4375f, 0.10625f, 0.45f, 0.88125f, 0.49375f, 0.31875f, 0.85625f, 0.8625f, 0.99375f, 0.8875f, 0.0375f, 0.125f, 0.38125f, 0.78125f, 0.975f, 
			0.6375f, 0.875f, 0.55625f, 0.23125f, 0.65625f, 0.36875f, 0.8875f, 0.2375f, 0.125f, 0.19375f, 0.85625f, 0.65f, 0.14375f, 0.85f, 0.53125f, 0.08125f, 
			0.7625f, 0.725f, 0.7625f, 0.7375f, 0.75625f, 0.0125f, 0.7f, 0.98125f, 0.74375f, 0.76875f, 0.94375f, 0.55625f, 0.175f, 0.36875f, 0.88125f, 0.43125f, 
			0.55625f, 0.3125f, 0.0875f, 0.95f, 0.575f, 0.60625f, 0.35f, 0.31875f, 0.325f, 0.64375f, 0.15625f, 0.09375f, 0.40625f, 0.9f, 0.30625f, 0.6625f, 0.05f, 
			0.275f, 0.41875f, 0.4125f, 0.075f, 0.49375f, 0.91875f, 0.66875f, 0.76875f, 0.26875f, 0.4625f, 0.86875f, 0.53125f, 0.75625f, 0.20625f, 0.2625f, 0.4875f, 
			0.95625f, 0.00625f, 0.6125f, 0.66875f, 0.55f, 0.84375f, 0.9375f, 0.11875f, 0.96875f, 0.64375f, 0.0625f, 0.63125f, 0.9875f, 0.78125f, 0.9375f, 0.1875f, 
			0.75f, 0.3875f, 0.30625f, 0.0375f, 0.56875f, 0.79375f, 0.9375f, 0.46875f, 0.3125f, 0.18125f, 0.88125f, 0.0125f, 0.71875f, 0.8875f, 0.9625f, 0.45625f, 
			0.50625f, 0.225f, 0.9625f, 0.10625f, 0.6375f, 0.54375f, 0.7625f, 0.7875f, 0.525f, 0.1625f, 0.88125f, 0.125f, 0.575f, 0.925f, 0.5625f, 0.43125f, 0.9f, 
			0.2f, 0.06875f, 0.8125f, 0.4875f, 0.85625f, 0.14375f, 0.85f, 0.8125f, 0.08125f, 0.35f, 0.4f, 0.01875f, 0.7375f, 0.33125f, 0.9625f, 0.78125f, 0.89375f, 
			0.58125f, 0.05f, 0.90625f, 0.33125f, 0.84375f, 0.05625f, 0.76875f, 0.79375f, 0.7125f, 0.15f, 0.7375f, 0.43125f, 0.5375f, 0.5f, 0.58125f};
		#endregion
		
		public void Rebuild ()
		{
			//re-loading data
			data.LoadFromByteList(data.compressed);
						
			//applying settings
			chunkSize = guiNewChunkSize;
			
			data.emptyColumn = new GeovoxData.Column(true); //sending boolean argument with fn will create a list
			data.emptyColumn.AddBlocks(guiEmptyColumnType, guiEmptyColumnHeight);
			
			//adding renderer if rtp compatibility is on
			if (rtpCompatible)
			{
				MeshRenderer renderer = GetComponent<MeshRenderer>();
				if (renderer==null) 
				{ 
					renderer = gameObject.AddComponent<MeshRenderer>(); 
					renderer.hideFlags = HideFlags.HideInInspector;
				}
				terrainMaterial = renderer.sharedMaterial;
			}
			
			//rebuilding
			chunks.Clear();
			Display(true);
		}
		
		public void AddPrefab(VoxelBlockType type){
			Transform[] newPrefabList = new Transform[type.prefabs.Length + 1];
			for (int i = 0; i < type.prefabs.Length; i++) {
				newPrefabList [i] = type.prefabs [i];
			}
			type.prefabs = newPrefabList;
			type.offset.Add(0);
			type.alignToNormals.Add(false);
			type.objectNormalArea.Add(0);
			type.randomRotation.Add(true);
			type.scaleRamp.Add (0);
			type.collidersActive.Add (true);
		}
		
		public void RemovePrefab(VoxelBlockType type){
			Transform[] newPrefabList = new Transform[type.prefabs.Length - 1];
			for (int i = 0; i < newPrefabList.Length; i++) {
				newPrefabList [i] = type.prefabs [i];
				
			}
			type.prefabs = newPrefabList;
			type.offset.RemoveAt(type.offset.Count - 1);
			type.alignToNormals.RemoveAt(type.alignToNormals.Count - 1);
			type.objectNormalArea.RemoveAt(type.objectNormalArea.Count - 1);
			type.randomRotation.RemoveAt(type.randomRotation.Count - 1);
			type.collidersActive.RemoveAt(type.collidersActive.Count - 1);
		}
	}


}//namespace