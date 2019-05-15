using UnityEngine;

[System.Serializable]
public class Chunk{
    private GameObject gameObject;
    private MeshFilter meshFilter;
    private bool active;
    private int positionX;
    private int positionZ;
    private bool needsUpdate;
    private int lod = 0;

    [SerializeField]
    private int northChunk = -1;
    [SerializeField]
    private int eastChunk = -1;
    [SerializeField]
    private int southChunk = -1;
    [SerializeField]
    private int westChunk = -1;

    public Mesh Mesh { get => meshFilter.mesh; }
    public MeshFilter MeshFilter { get => meshFilter; }
    public bool Active { get => active; }
    public bool NeedsUpdate { get => needsUpdate; }
    public int PositionX { get => positionX; }
    public int PositionZ { get => positionZ; }
    public Vector3 position { get => new Vector3(positionX, 0, positionZ); }
    public int Lod {
        get {
            return lod;
        }
        set{
            if (value != lod) {
                needsUpdate = true;
            }
            lod = value;
        }
    }
    public int NorthChunk { get => northChunk; }
    public int EastChunk { get => eastChunk; }
    public int SouthChunk { get => southChunk; }
    public int WestChunk { get => westChunk; }

    public Chunk(MeshFilter meshFilter, Mesh mesh, int positionX, int positionZ, int northChunk, int eastChunk, int southChunk, int westChunk) {
        this.gameObject = meshFilter.gameObject;
        this.meshFilter = meshFilter;
        this.meshFilter.mesh = mesh;
        this.active = false;
        this.needsUpdate = true;
        this.positionX = positionX;
        this.positionZ = positionZ;
        this.northChunk = northChunk;
        this.eastChunk = eastChunk;
        this.southChunk = southChunk;
        this.westChunk = westChunk;
    }

    public void Update(Mesh mesh) {
        this.meshFilter.mesh = mesh;
        this.needsUpdate = false;
    }

    public void SetActive(bool value) {
        gameObject.SetActive(value);
        needsUpdate = false;
        active = value;
    }
}
