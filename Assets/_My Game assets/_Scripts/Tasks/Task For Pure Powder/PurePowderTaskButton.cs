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
            }
        }
    }

    private void OnMouseUp()
    {
        buttonClick = true;
        currentMaterial.material = purePowderTask.neutralColourMaterial;

        purePowderTask.AddColour(buttonMaterial);
    }

}
