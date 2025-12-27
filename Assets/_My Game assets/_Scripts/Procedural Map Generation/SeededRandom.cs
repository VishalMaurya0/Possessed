using UnityEngine;

public class SeededRandom
{
    private System.Random rng;

    public SeededRandom(int seed)
    {
        rng = new System.Random(seed);
    }

    // Helper for Integers
    public int Range(int min, int max)
    {
        return rng.Next(min, max);
    }

    // Helper for Floats
    public float Range(float min, float max)
    {
        return (float)rng.NextDouble() * (max - min) + min;
    }

    public Quaternion Rotation()
    {
        // Generates a completely random rotation
        return Quaternion.Euler(
            (float)rng.NextDouble() * 360f,
            (float)rng.NextDouble() * 360f,
            (float)rng.NextDouble() * 360f
        );
    }
}