using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Class for handling networked player behaviour in the bubblle blowing stage.</summary>
public class NetworkPlayerBlow : NetworkBehaviour {

    /// <summary>
    /// Struct to hold the keyboard input name and keycode.</summary>
    [System.Serializable]
    public struct KeyName {

        /// <summary>
        /// Name of the input button.</summary>
        public string ButtonName;

        /// <summary>
        /// The keyboard keycode for the input button.</summary>
        public string KeyCode;
    };

    /// <summary>
    /// Struct to hold axis names for user input.</summary>
    [System.Serializable]
    public struct Axes {

        /// <summary>
        /// Left button to blow up bubbleman.</summary>
        public KeyName BlowLeft;

        /// <summary>
        /// Right button to blow up bubbleman.</summary>
        public KeyName BlowRight;
    };

    /// <summary>
    /// Audio source for blow SFX.</summary>
    public AudioSource Source;

    /// <summary>
    /// The blow SFX clip.</summary>
    public AudioClip BlowSoundClip;

    /// <summary>
    /// Axes for user input.</summary>
    public Axes InputAxes;

    /// <summary>
    /// The tag that is above the player's head.</summary>
    public GameObject PlayerTag;

    /// <summary>
    /// Amount by which to increment the bubbleman's size.</summary>
    public float BlowStep = 0.02F;

    /// <summary>
    /// Is the next button the player needs to press the left blow button.</summary>
    private bool nextIsLeftButton = true;

    /// <summary>
    /// This function is called on the frame when a script is enabled just before any of the Update methods is called the first time.</summary>
    private void Start() {
        if (!isLocalPlayer) {
            return;
        }

        // turn on player tag
        PlayerTag.SetActive(true);
        PlayerTag.GetComponent<Text>().color = (name == "Player 1") ? Color.red : Color.yellow;
    }

    /// <summary>
    /// This function is called every frame, if the <c>MonoBehaviour</c> is enabled.</summary>
    private void Update() {
        if (!isLocalPlayer) {
            return;
        }

        // check for which button the player should press
        string nextInput = nextIsLeftButton ? InputAxes.BlowLeft.ButtonName : InputAxes.BlowRight.ButtonName;

        if (Input.GetButtonUp(nextInput)) {
            // blow up bubbleman a tiny bit
            CmdBlowBubbleman(gameObject);

            // use other button next time
            nextIsLeftButton = !nextIsLeftButton;
        }
    }

    /// <summary>
    /// Tells the server to incrementally increase the size of the bubbleman.</summary>
    /// <param name="player">The player's bubbleman.</param>
    [Command]
    private void CmdBlowBubbleman(GameObject player) {
        RpcBlowBubbleman(player);
    }

    /// <summary>
    /// Tells all clients to incrementally increase the size of the bubbleman.</summary>
    /// <param name="player">The player's bubbleman.</param>
    [ClientRpc]
    private void RpcBlowBubbleman(GameObject player) {
        player.transform.localScale += new Vector3(BlowStep, BlowStep, BlowStep);
        player.GetComponent<NetworkPlayerBlow>().Source.PlayOneShot(player.GetComponent<NetworkPlayerBlow>().BlowSoundClip, 1.0F);
    }
}
