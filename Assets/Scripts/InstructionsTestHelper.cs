using UnityEngine;

/// <summary>
/// Helper script for testing instructions behavior
/// Add this as a component to any GameObject for easy testing
/// </summary>
public class InstructionsTestHelper : MonoBehaviour
{
    [Header("Instructions Testing Tools")]
    [SerializeField] private bool showTestButtons = true;
    
    private void OnGUI()
    {
        if (!showTestButtons) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Instructions Testing Tools:");
        
        if (GUILayout.Button("Reset Instructions Flag (Will Show Again)"))
        {
            InstructionsManager.ResetInstructionsFlag();
        }
        
        if (GUILayout.Button("Permanently Disable Instructions"))
        {
            InstructionsManager.PermanentlyDisableInstructions();
        }
        
        // Show current status
        bool instructionsShown = PlayerPrefs.GetInt("InstructionsShown", 0) == 1;
        GUILayout.Label("Current Status: " + (instructionsShown ? "Instructions DISABLED" : "Instructions ENABLED"));
        
        GUILayout.EndArea();
    }
}