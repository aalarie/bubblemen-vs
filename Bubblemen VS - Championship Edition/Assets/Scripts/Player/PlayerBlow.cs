using UnityEngine;

/// <summary>
/// Class for handling player behaviour in the bubblle blowing stage.</summary>
public class PlayerBlow : MonoBehaviour {

    // Audio Source
    public AudioSource audioSource;
    public AudioClip blowSoundClip;

    /// <summary>
    /// Struct to hold the keyboard input name and keycode.</summary>
    [System.Serializable]
    public struct KeyName
    {
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
    /// Axes for user input.</summary>
    public Axes InputAxes;

    /// <summary>
    /// Amount by which to increment the bubbleman's size.</summary>
    public float BlowStep = 0.02F;

    /// <summary>
    /// Is the next button the player needs to press the left blow button.</summary>
    private bool nextIsLeftButton = true;

    /// <summary>
    /// This function is called every frame, if the <c>MonoBehaviour</c> is enabled.</summary>
    private void Update() {
        // check for which button the player should press
        string nextInput = nextIsLeftButton ? InputAxes.BlowLeft.ButtonName : InputAxes.BlowRight.ButtonName;
        
        if (Input.GetButtonUp(nextInput)) {
            // blow up bubbleman a tiny bit
            transform.localScale += new Vector3(BlowStep, BlowStep, BlowStep);

            // use other button next time
            nextIsLeftButton = !nextIsLeftButton;

            // Play Sound
            audioSource.PlayOneShot(blowSoundClip, 1.0f);
        }
    }
}
