using UnityEngine;

public class LeaveSessionProxy : MonoBehaviour
{
    // This method will trigger the internal Leave() method on the LeaveSession component
    public void TriggerLeave()
    {
        SendMessage("Leave"); // Calls the internal method via Unity message system
    }
}
