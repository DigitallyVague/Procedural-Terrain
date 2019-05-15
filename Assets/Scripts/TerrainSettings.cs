using UnityEngine;

namespace DigitallyVague.Terrain {
    [CreateAssetMenu]
    public class TerrainSettings : ScriptableObject {
        [SerializeField]
        private int chunkSize = 32;
        [SerializeField]
        private int chunkCountX = 4;
        [SerializeField]
        private int chunkCountZ = 4;

        [SerializeField]
        private NoiseLayer[] noiseLayers = new NoiseLayer[0];

        [SerializeField]
        private int seed = 0;
        [SerializeField]
        private bool useRandomSeed = false;

        [SerializeField]
        private Gradient colors = new Gradient();

        public int ChunkSize { get => chunkSize; }
        public int ChunkCountX { get => chunkCountX; }
        public int ChunkCountZ { get => chunkCountZ; }
        public int Seed {
            get{
                if (useRandomSeed) { return Random.Range(0, 10000); }
                return seed;
            }
        }
        public Gradient Colors { get => colors; }

        public float SampleNoise(float x, float z) {
            float value = 0;
            for (int i = 0; i < noiseLayers.Length; i++) {
                switch (noiseLayers[i].BlendMode) {
                    case NoiseBlendMode.Add:
                    value += noiseLayers[i].Sample(x, z);
                    break;
                    case NoiseBlendMode.Multiply:
                    value *= noiseLayers[i].Sample(x, z);
                    break;
                    default:
                    value += noiseLayers[i].Sample(x, z);
                    break;
                }
                
            }
            return value;
        }
    }
}