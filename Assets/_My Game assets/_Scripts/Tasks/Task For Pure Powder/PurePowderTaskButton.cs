using UnityEngine;
using UnityEngine.Android;

public class PurePowderTaskButton : MonoBehaviour
{
    public PurePowderTask purePowderTask;
    public Material buttonMaterial;
    public MeshRenderer currentMaterial;
    bool buttonClick;
    float responseTime = 0.2f;
    float time;

    private void Start()
    {
        purePowderTask = GetComponentInParent<PurePowderTask>();
        currentMaterial = GetComponent<MeshRenderer>();
        buttonMaterial = currentMaterial.sharedMaterial;    //TODO Champt GPT TODO//

        Debug.LogWarning($"Button '{gameObject.name}' initialized with material: {buttonMaterial.name}");
    }

    private void Update()
    {
        if (buttonClick)
        {
            time += Time.deltaTime;
            if (time > responseTime)
            {
                time = 0;
                buttonClick = false;
                currentMaterial.material = buttonMaterial;
                Debug.LogWarning($"Button '{gameObject.name}' material reset to original: {buttonMaterial.name}");
            }
        }
    }

    private void OnMouseUp()
    {
        buttonClick = true;
        currentMaterial.material = purePowderTask.neutralColourMaterial;
        Debug.LogWarning($"Button '{gameObject.name}' clicked. Temp material set to: {purePowderTask.neutralColourMaterial.name}");

        purePowderTask.AddColour(buttonMaterial);
    }

}
