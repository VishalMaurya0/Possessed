using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

public class Player2Setup : MonoBehaviour
{
// using ParrelSync; // Uncomment if you are using ParrelSync

public async void Start() 
{
    var options = new InitializationOptions();

    // ---------------------------------------------------------
    // LOGIC TO SWITCH PROFILE
    // ---------------------------------------------------------
    
    // Scenario A: If you are using ParrelSync (Recommended)
    // if (ClonesManager.IsClone()) 
    // {
    //     options.SetProfile("Clone_User");
    // }
    
    // Scenario B: Quick fix (Manual Toggle)
    // Toggle this boolean in the Inspector for your second editor only
    if (useSecondaryProfile) 
    {
        options.SetProfile("Player2_Profile");
    }
    else
    {
        options.SetProfile("Player1_Profile");
    }

    // ---------------------------------------------------------

    await UnityServices.InitializeAsync(options);

    // Now Sign In
    await AuthenticationService.Instance.SignInAnonymouslyAsync();
    
    Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
}

// Add this variable to your script to manually check inside the Inspector
[SerializeField] private bool useSecondaryProfile = false;
}
