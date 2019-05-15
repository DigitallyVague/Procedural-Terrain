using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitallyVague.Terrain {
    public class ProceduralTerrain : MonoBehaviour {
        [Header("Settings"), SerializeField]
        private TerrainSettings terrainSettings = null;
        private int seed = 0;
        [SerializeField]
        private Material terrainMaterial = null;
        [SerializeField]
        private int maxLOD = 0;
        [SerializeField]
        private int minLOD = 3;

        public int MaxLOD { get => maxLOD; }
        public int MinLOD { get => minLOD; }   
        
        private List<LODLevelInfo> lodLevels = new List<LODLevelInfo>();

        [SerializeField]
        private Transform viewer = null;
        private Vector3 viewPosition = Vector3.zero;
        private Vector3 lastViewPosition = Vector3.zero;
        private Vector3 halfChunkSize = Vector3.zero;

        private Chunk[] chunks = new Chunk[0];

        [Header("Debug:"), SerializeField]
        private bool showChunks = true;
        [SerializeField]
        private Gradient lodColorGradient = new Gradient();


        void Start() {
            halfChunkSize = new Vector3(terrainSettings.ChunkSize * 0.5f, 0, terrainSettings.ChunkSize * 0.5f);
            viewPosition -= halfChunkSize;
            lastViewPosition = viewPosition + Vector3.one;

            if (terrainSettings != null) {
                seed = terrainSettings.Seed;
                CreateChunks();
            } else {
                Debug.LogError("No Terrain Settings");
            }
        }

        private void Update() {
            viewPosition = RoundToChunkPosition(viewer.position - halfChunkSize);
            if (lastViewPosition != viewPosition) {
                CalculateLOD();
                lastViewPosition = viewPosition;
            }

            for (int i = 0; i < chunks.Length; i++) {
                if (chunks[i].NeedsUpdate) {
                    GenerateChunk(chunks[i]);
                }
            }
        }

        private void CalculateLOD() {
            for (int i = 0; i < chunks.Length; i++) {
                float dist = Vector3.Distance(chunks[i].position * terrainSettings.ChunkSize, viewPosition);
                if (dist <= 128) {
                    chunks[i].Lod = 0;
                } else if (dist > 128 && dist <= 256) {
                    chunks[i].Lod = 1;
                } else if(dist > 256 && dist <= 384){
                    chunks[i].Lod = 2;
                } else if(dist > 384){
                    chunks[i].Lod = 3;
                }
            }
        }

        /// <summary>
        /// Generates each 
        /// </summary>
        private void CreateChunks() {
            int chunkCount = terrainSettings.ChunkCountX * terrainSettings.ChunkCountZ;
            chunks = new Chunk[chunkCount];

            for (int x = 0; x < terrainSettings.ChunkCountX; x++) {
                for (int z = 0; z < terrainSettings.ChunkCountZ; z++) {
                    int index = (x * terrainSettings.ChunkCountZ) + z;
                    GameObject chunkGameobject = new GameObject();
                    chunkGameobject.transform.SetParent(transform);
                    chunkGameobject.hideFlags = HideFlags.HideInHierarchy;
                    MeshFilter meshFilter = chunkGameobject.AddComponent<MeshFilter>();
                    chunkGameobject.AddComponent<MeshRenderer>().material = terrainMaterial;
                    Mesh mesh = new Mesh();
                    mesh.name = chunkGameobject.name;

                    int northChunkIndex = GetChunkNeighborIndex(x, z + 1);
                    int eastChunkIndex = GetChunkNeighborIndex(x + 1, z);
                    int southChunkIndex = GetChunkNeighborIndex(x, z - 1);
                    int westChunkIndex = GetChunkNeighborIndex(x - 1, z);

                    Chunk chunk = new Chunk(meshFilter, mesh, x, z, northChunkIndex, eastChunkIndex, southChunkIndex, westChunkIndex);
                    chunk.Lod = 4;
                    chunks[index] = chunk;
                }
            }
        }

        private void GenerateChunk(Chunk chunk) {
            LODLevelInfo lodLevel = GetLODLevel(chunk.Lod);

            int lodStep = lodLevel.StepSize;
            int chunkSize = lodLevel.ChunkSize;
            float halfChunkSize = terrainSettings.ChunkSize * 0.5f;

            Vector3[] verticies = new Vector3[lodLevel.VertexCount];
            int[] triangles = new int[lodLevel.TriangleCount];
            Color[] colors = new Color[verticies.Length];

            int vertCount = 0;
            int triangleCount = 0;
            int vertexIndex = 0;

            for (int x = 0; x <= terrainSettings.ChunkSize; x += lodStep) {
                for (int z = 0; z <= terrainSettings.ChunkSize; z += lodStep) {
                    float offsetX = chunk.PositionX  * terrainSettings.ChunkSize + x;
                    float offsetZ = chunk.PositionZ * terrainSettings.ChunkSize + z;

                    Vector3 vertex = new Vector3(offsetX, terrainSettings.SampleNoise(offsetX + seed, offsetZ + seed), offsetZ);

                    verticies[vertexIndex] = vertex;
                    colors[vertexIndex] = terrainSettings.Colors.Evaluate(Mathf.Clamp01(vertex.y / 80f));
                    vertexIndex++;

                    if (x < terrainSettings.ChunkSize && z < terrainSettings.ChunkSize) {
                        triangles[triangleCount + 0] = (vertCount + 0);
                        triangles[triangleCount + 1] = (vertCount + 1);
                        triangles[triangleCount + 2] = (vertCount + chunkSize + 2);


                        triangles[triangleCount + 3] = (vertCount + 0);
                        triangles[triangleCount + 4] = (vertCount + chunkSize + 2);
                        triangles[triangleCount + 5] = (vertCount + chunkSize + 1);

                        vertCount++;
                        triangleCount += 6;
                    }
                }
                vertCount++;
            }

            Mesh chunkMesh = chunk.Mesh;
            chunkMesh.Clear();
            chunkMesh.vertices = verticies;
            chunkMesh.SetTriangles(triangles, 0);
            chunkMesh.colors = colors;
            chunkMesh.RecalculateNormals();
            chunk.Update(chunkMesh);
        }

        private Vector3 RoundToChunkPosition(Vector3 position) {
            return new Vector3(Mathf.Round(position.x / terrainSettings.ChunkSize) * terrainSettings.ChunkSize, 0, Mathf.Round(position.z / terrainSettings.ChunkSize) * terrainSettings.ChunkSize);
        }

        /// <summary>
        /// returns the index of the neighboring chunk if one exists otherwise it will return -1.
        /// </summary>
        /// <param name="x">Chunks X position in grid not world space.</param>
        /// <param name="z">Chunks Z position in grid not world space.</param>
        int GetChunkNeighborIndex(int x, int z) {
            int result = -1;

            if (x >= 0 && x < terrainSettings.ChunkCountX && z >= 0 && z < terrainSettings.ChunkCountZ) {
                result = (x * terrainSettings.ChunkCountZ) + z;
            }

            return result;
        }

        /// <summary>
        /// returns LOD level info based on the given lod level and is clamped between the min and max LOD.
        /// </summary>
        /// <param name="lod"></param>
        /// <returns></returns>
        private LODLevelInfo GetLODLevel(int lod) {
            int index = Mathf.Clamp(lod, maxLOD, Mathf.Min(minLOD, lodLevels.Count - 1));
            return lodLevels[index];
        }

        private void OnDrawGizmos() {
            Vector3 chunkScale = new Vector3(terrainSettings.ChunkSize * 0.96f, 0, terrainSettings.ChunkSize * 0.96f);
            float chunkPostionOffset = terrainSettings.ChunkSize * 0.5f;

            if (showChunks && terrainSettings != null) {
                if (chunks != null && chunks.Length > 0) {
                    Gizmos.color = Color.red;
                    for (int i = 0; i < chunks.Length; i++) {
                        Vector3 chunkPosition = new Vector3(chunks[i].PositionX * terrainSettings.ChunkSize + chunkPostionOffset, 0, chunks[i].PositionZ * terrainSettings.ChunkSize + chunkPostionOffset);
                        Gizmos.color = lodColorGradient.Evaluate((float)chunks[i].Lod / (float)minLOD);
                        Gizmos.DrawWireCube(chunkPosition, chunkScale);
                    }
                } else {
                    Gizmos.color = Color.black;
                    for (int x = 0; x < terrainSettings.ChunkCountX; x++) {
                        for (int z = 0; z < terrainSettings.ChunkCountZ; z++) {
                            Gizmos.DrawWireCube(new Vector3(x * terrainSettings.ChunkSize + chunkPostionOffset, 0, z * terrainSettings.ChunkSize + chunkPostionOffset), chunkScale);
                        }
                    }
                }

            }
        }

        private void OnValidate() {
            lodLevels.Clear();
            
            for (int i = 1; i < terrainSettings.ChunkSize; i++) {
                int value = i;
                if (terrainSettings.ChunkSize % value == 0) {
                    lodLevels.Add(new LODLevelInfo(value, terrainSettings.ChunkSize));
                }
            }
        }
    }

    [System.Serializable]
    public class LODLevelInfo {
        private int stepSize = 1;
        private int chunkSize = 0;
        private int vertexCount = 0;
        private int triangleCount = 0;

        public int StepSize { get => stepSize; }
        public int ChunkSize { get => chunkSize; }
        public int VertexCount { get => vertexCount; }
        public int TriangleCount { get => triangleCount; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepSize"></param>
        /// <param name="distance"></param>
        /// <param name="chunkSize">Size of the chunk as specified in the TerrainSettings</param>
        public LODLevelInfo(int stepSize, float chunkSize) {
            this.stepSize = stepSize;
            this.chunkSize = Mathf.RoundToInt(chunkSize / stepSize);
            vertexCount = (this.chunkSize + 1) * (this.chunkSize + 1);
            triangleCount = this.chunkSize * this.chunkSize * 6;
        }
    }
}