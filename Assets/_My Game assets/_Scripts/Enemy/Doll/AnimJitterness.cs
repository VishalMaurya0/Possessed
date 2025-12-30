using UnityEngine;

public class AnimJitterness : MonoBehaviour
{
    public Animator animator;
    public float frameRate = 12f; // 12 FPS is standard for "anime/stop motion" feel

    private float timer;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        // Disable automatic updates so we can control it manually
        animator.enabled = false; 
    }

    void Update()
    {
        // Accumulate time
        timer += Time.deltaTime;

        // Calculate the time per frame (e.g., 1/12 = 0.0833 seconds)
        float updateRate = 1f / frameRate;

        // If enough time has passed, update the animator
        while (timer >= updateRate)
        {
            // Manually advance the animation
            animator.Update(updateRate);
            timer -= updateRate;
        }
    }
}