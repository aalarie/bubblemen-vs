using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This script will auto-ready the player once they've connected to the lobby.</summary>
public class AutoReadyUp : MonoBehaviour {

    /// <summary>
    /// This function is called on the frame when a script is enabled just before any of the Update methods is called the first time.</summary>
    private void Start() {
        // send ready message as soon as the player is created
        GetComponent<NetworkLobbyPlayer>().SendReadyToBeginMessage();
    }
}
