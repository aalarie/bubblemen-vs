using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles button behaviour for UI elements.</summary>
public class ButtonManager : MonoBehaviour {

    // Menu audio
    public AudioSource menuAudio;
    public AudioClip buttonConfirmationClip;

    // Menu Load Panel
    public GameObject loadPanel;

    // Game Escape Panel
    public GameObject escapePanel;

    /// <summary>
    /// Set of UI elements for use in online mode.</summary>
    [System.Serializable]
    public struct MultiplayerUI {

        /// <summary>
        /// Text element to show status feedback.</summary>
        public Text StatusText;

        /// <summary>
        /// The panel which contains multiplayer options.</summary>
        public GameObject MultiplayerPanel;

        /// <summary>
        /// The button to start hosting a match.</summary>
        public GameObject HostMatchButton;

        /// <summary>
        /// The button to start searching for a match.</summary>
        public GameObject JoinMatchButton;

        /// <summary>
        /// The button to stop hosting/joining a match.</summary>
        public GameObject StatusPanel;
    };

    /// <summary>
    /// The UI elements for online mode.</summary>
    public MultiplayerUI Multiplayer;

    /// <summary>
    /// The name of the selected arena.</summary>
    /// <remarks>Used in online mode only.</remarks>
    private string selectedArena;

    /// <summary>
    /// Whether the client is host a match or joining one.</summary>
    private bool isHosting;

    /// <summary>
    /// Returns the player to the main menu.</summary>
    public void ReturnToMainMenu() {
        StartCoroutine(loadNewSceneWithSound("MainMenu"));
    }

    /// <summary>
    /// Moves the player from the main menu to the pre-game scene for local multiplayer mode.</summary>
    public void MainMenuToPreGameLocal() {
        StartCoroutine(loadNewSceneWithSound("PreGameLocal"));
    }

    /// <summary>
    /// Moves the player from the main menu to the pre-game scene for VS AI mode.</summary>
    public void MainMenuToPreGameVsAI() {
        // TODO: implement
    }

    /// <summary>
    /// Moves the player from the main menu to the pre-game scene for online multiplayer mode.</summary>
    public void MainMenuToPreGameOnline() {
        StartCoroutine(loadNewSceneWithSound("PreGameOnline"));
    }

    /// <summary>
    /// Moves the player from the pre-game for local multiplayer mode to the sink arena.</summary>
    public void PreGameLocalToSinkArena() {
        loadPanel.SetActive(true);
        StartCoroutine(loadNewSceneWithSound("SinkArena"));
    }

    /// <summary>
    /// Moves the player from the pre-game for VS AI mode to the sink arena.</summary>
    public void PreGameVsAIToSinkArena() {
        // TODO: implement
    }

    public void SinkArenaToPreGameLocal() {
        escapePanel.SetActive(true);
        SceneManager.LoadScene("PreGameLocal");
    }

    public void SinkArenaToPreGameOnline() {
        NetworkManager.singleton.StopClient();
    }

    /// <summary>
    /// Sets the selected arena variable and shows the multiplayer menu.</summary>
    public void PreGameOnlineOnArenaSelected(string arena) {
        switch (arena) {
        case "sink":
            selectedArena = arena;

            // show multiplayer options
            menuAudio.PlayOneShot(buttonConfirmationClip);
            Multiplayer.MultiplayerPanel.SetActive(true);
            break;
        default:
            Debug.LogErrorFormat("Arena \"{0}\" does not exist", arena);
            break;
        }
    }

    /// <summary>
    /// Creates a room for a match ands waits for one other player.</summary>
    public void PreGameOnlineOnHostMatch() {
        menuAudio.PlayOneShot(buttonConfirmationClip);
        Multiplayer.StatusPanel.SetActive(true);

        // host a multiplayer match
        isHosting = true;
        GameObject.Find("Online Manager").GetComponent<OnlineManager>().HostMatch(Multiplayer.StatusText);
    }

    /// <summary>
    /// Waits for an open multiplayer room to become available and attemps to join it.</summary>
    public void PreGameOnlineOnJoinMatch() {
        menuAudio.PlayOneShot(buttonConfirmationClip);
        Multiplayer.StatusPanel.SetActive(true);

        // try to join a multiplayer match
        isHosting = false;
        GameObject.Find("Online Manager").GetComponent<OnlineManager>().JoinMatch(Multiplayer.StatusText);
    }

    /// <summary>
    /// Cancels the current online action.</summary>
    public void PreGameOnlineOnCancel() {
        Multiplayer.StatusPanel.SetActive(false);

        // cancel current online action (i.e. host/join match)
        if (isHosting) {
            GameObject.Find("Online Manager").GetComponent<OnlineManager>().CancelHostMatch();
            Multiplayer.MultiplayerPanel.SetActive(false);
        } else {
            GameObject.Find("Online Manager").GetComponent<OnlineManager>().CancelJoinMatch();
            Multiplayer.MultiplayerPanel.SetActive(false);
        }
        Multiplayer.StatusText.text = "";
    }

    public void SinkArenaEscapeOnCancel() {
        escapePanel.SetActive(false);
    }

    IEnumerator loadNewSceneWithSound(string sceneToLoad) {
        // Play button sound
        menuAudio.PlayOneShot(buttonConfirmationClip);

        // Once announcer sound is done load scene
        yield return new WaitForSeconds(buttonConfirmationClip.length);
        SceneManager.LoadScene(sceneToLoad);
    }
}
