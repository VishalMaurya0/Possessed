using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        if (!GameManager.Instance.lockCurser)
        {
            UnlockCursor();
        }
        else if (GameManager.Instance.lockCurser)
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
