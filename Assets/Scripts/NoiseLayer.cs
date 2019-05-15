using UnityEngine;

namespace DigitallyVague.Terrain {
    [CreateAssetMenu]
    public class NoiseLayer : ScriptableObject{
        [SerializeField]
        private float amplitude = 1;
        [SerializeField]
        private float frequency = 0.1f;
        [SerializeField]
        private int octaves = 4;
        [SerializeField]
        private float persistence = 0.5f;
        [SerializeField]
        private float lacunarity = 1.5f;
        [SerializeField]
        private AnimationCurve falloffCurve = new AnimationCurve();
        [SerializeField]
        private NoiseBlendMode blendMode = NoiseBlendMode.Add;

        public NoiseBlendMode BlendMode { get => blendMode; }

        public float Sample(float x, float z) {
            float value = 0;
            float tempAmplitude = amplitude;
            float tempFrequency = frequency;

            for (int i = 0; i < octaves; i++) {
                float perlin = falloffCurve.Evaluate(Mathf.PerlinNoise(x * tempFrequency, z * tempFrequency));
                value += perlin * tempAmplitude;
                tempAmplitude *= persistence;
                tempFrequency *= lacunarity;
            }

            return value;
        }
    }

    public enum NoiseBlendMode { Add, Multiply }
}