using UnityEngine;

public class RotateYMatcher : MonoBehaviour
{
    [Tooltip("Assign the object you want to follow here.")]
    public Transform referenceObject;

    [Tooltip("If true, this object stays perfectly upright (X and Z are zero). If false, it keeps its own X/Z tilt.")]
    public bool lockVertical = false;

    void Update()
    {
        // 1. Safety check to prevent crash if slot is empty
        if (referenceObject == null) return;

        // 2. Get the current rotation of this object
        Vector3 currentEuler = transform.eulerAngles;

        // 3. Get the Y rotation from the reference
        float targetY = referenceObject.eulerAngles.y;

        // 4. Determine final X and Z based on settings
        float finalX = lockVertical ? 0f : currentEuler.x;
        float finalZ = lockVertical ? 0f : currentEuler.z;

        // 5. Apply the new rotation
        transform.eulerAngles = new Vector3(finalX, targetY, finalZ);
    }
}