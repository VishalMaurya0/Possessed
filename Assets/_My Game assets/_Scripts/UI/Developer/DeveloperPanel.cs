using TMPro;
using UnityEngine;

public class DeveloperPanel : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text FPSText;
    public float wait = 0;

    private void Update() {
        if (Input.GetKey(KeyCode.V) && Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.H) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.L) && wait < 0) {
            panel.gameObject.SetActive(!panel.gameObject.activeSelf);
            wait = 0.1f;
        }
        wait -= Time.unscaledDeltaTime;


        float fps = 1.0f / Time.unscaledDeltaTime;
        FPSText.text = "FPS: " + Mathf.CeilToInt(fps).ToString();
    }

    public void TestingMode()
    {

    }
}
