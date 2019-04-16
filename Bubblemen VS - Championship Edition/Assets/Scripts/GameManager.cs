using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles game logic for Bubblemen VS.</summary>
public class GameManager : MonoBehaviour {

    // Game AudioSource
    public AudioSource audioSource;

    // AudioClips
    public AudioClip announcer_3;
    public AudioClip announcer_2;
    public AudioClip announcer_1;
    public AudioClip announcer_blow;
    public AudioClip announcer_fight;

    public GameObject escapePanel;

    /// <summary>
    /// Bubblemen VS game stages.</summary>
    public enum GameStage {

        /// <summary>
        /// Before the bubble blowing stage.</summary>
        PreBubbleBlowing,

        /// <summary>
        /// Bubble blowing stage.</summary>
        BubbleBlowing,

        /// <summary>
        /// Fighting stage.</summary>
        Fighting,

        /// <summary>
        /// Post-fighting stage.</summary>
        PostFighting
    };

    /// <summary>
    /// Player 1's game object.</summary>
    public GameObject PlayerOne;

    /// <summary>
    /// Player 2's game object.</summary>
    public GameObject PlayerTwo;

    /// <summary>
    /// Text element located at the top of the screen.</summary>
    public Text TopText;

    /// <summary>
    /// The current stage of the game.</summary>
    public GameStage CurrentStage;

    /// <summary>
    /// Player 1 btn </summary>
    public Button PlayerOneBtn1;

    /// <summary>
    /// The current stage of the game.</summary>
    public Button PlayerOneBtn2;

    /// <summary>
    /// Player 1's score.</summary>
    public int PlayerOneScore = 0;

    /// <summary>
    /// Player 2's score.</summary>
    public int PlayerTwoScore = 0;

    /// <summary>
    /// The score to reach to win the game.</summary>
    public int WinScore = 5;

    /// <summary>
    /// Game timer.</summary>
    private float timer;

    /// <summary>
    /// Blow up stage countdown </summary>
    private int countdown;

    /// <summary>
    /// blow up stage countdown seconds delay between decreasing number.</summary>
    private float countdownSecs;

    /// <summary>
    /// This is the time sec delay before lighting the alterate key.</summary>
    private float blowKeys;

    /// <summary>
    /// This is to light up and alternative the blowKeys for each player.</summary>
    private bool alterateKeys;

    /// <summary>
    /// This function is called on the frame when a script is enabled just before any of the Update methods is called the first time.</summary>
    private void Start() {
        // start game off in the bubble blowing stage
        CurrentStage = GameStage.PreBubbleBlowing;
        timer = 5F;
        countdownSecs = 1F;
        countdown = 4;
        blowKeys = 0.5F;
        alterateKeys = false;

        // set the position of the countdown to the centre of the arena
        TopText.rectTransform.anchorMin = new Vector2(0.5F, 0.5F);
        TopText.rectTransform.anchorMax = new Vector2(0.5F, 0.5F);
        TopText.rectTransform.pivot = new Vector2(0.5F, 0.5F);
        PlayerOneBtn1.GetComponentInChildren<Text>().text = "Left";
        PlayerOneBtn2.GetComponentInChildren<Text>().text = "Right";
        hideButtonBlowControls(false);
    }

    /// <summary>
    /// This function is called every frame, if the <c>MonoBehaviour</c> is enabled.</summary>
    private void Update() {
        switch (CurrentStage) {
        case GameStage.PreBubbleBlowing:
            UpdateBlowingPhaseCountdown();
            break;
        case GameStage.BubbleBlowing:
            UpdateBubbleBlowing();
            break;
        case GameStage.Fighting:
            UpdateFighting();
            break;
        case GameStage.PostFighting:
            UpdatePostFighting();
            break;
        }

        // Listen for escape press
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (escapePanel.activeSelf == true) {
                escapePanel.SetActive(false);
            } else {
                escapePanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// This function is called every frame in the pre blow up stage.</summary>
    private void UpdateBlowingPhaseCountdown() {
        //This will allow for a 1 second delay between each number in the countdown
        countdownSecs -= Time.deltaTime;
        if (countdownSecs > 0) {
            if (countdown == 4) {
                TopText.text = "";//"Ready in: ";
            } else if (countdown > 0) {
                TopText.text = "" + countdown;
            }
        } else {
            countdown--;
            countdownSecs = 1F;

            //Print BLOW! if we have reached the end of the countdown
            if (countdown == 3) {
                audioSource.PlayOneShot(announcer_3, 1.0f);
            } else if (countdown == 2) {
                audioSource.PlayOneShot(announcer_2, 1.0f);
            } else if (countdown == 1) {
                audioSource.PlayOneShot(announcer_1, 1.0f);
            } else if (countdown == 0) {
                TopText.text = "BLOW!";
                audioSource.PlayOneShot(announcer_blow, 1.0f);
            }

            //This is to allow for a 1sec delay of the text BLOW! before the start of the timer
            else if (countdown == -1) {
                PlayerOne.GetComponent<PlayerBlow>().enabled = true;
                PlayerTwo.GetComponent<PlayerBlow>().enabled = true;
                CurrentStage = GameStage.BubbleBlowing;
                hideButtonBlowControls(true);
            }
        }
    }

    /// <summary>
    /// This function is called every frame in the bubble blowing stage.</summary>
    private void UpdateBubbleBlowing() {
        // decrement bubble blowing timer
        timer -= Time.deltaTime;
        blowKeys -= Time.deltaTime;
        TopText.rectTransform.anchorMin = new Vector2(0.5F, 0.98F);
        TopText.rectTransform.anchorMax = new Vector2(0.5F, 0.98F);
        TopText.rectTransform.pivot = new Vector2(0.5F, 0.98F);

        if (timer > 0) {
            // update clock
            //TopText.text = "Timer\n" + System.Math.Round(timer, 0);
            TopText.text = "";
            if (blowKeys > 0 && !alterateKeys) {
                PlayerOneBtn1.GetComponent<Image>().color = Color.cyan;
                PlayerOneBtn2.GetComponent<Image>().color = Color.white;
            }
            if (blowKeys < 0) {
                alterateKeys = alterateKeys ? false : true;
                PlayerOneBtn2.GetComponent<Image>().color = Color.cyan;
                PlayerOneBtn1.GetComponent<Image>().color = Color.white;
                blowKeys = 0.5F;
            }
        } else {
            TopText.text = "Time is up!";
            hideButtonBlowControls(false);

            // activate player one
            PlayerOne.GetComponent<Rigidbody>().mass = PlayerOne.transform.localScale.x;
            PlayerOne.GetComponent<Rigidbody>().useGravity = true;
            PlayerOne.GetComponent<PlayerBlow>().enabled = false;
            PlayerOne.GetComponent<PlayerController>().enabled = true;

            // activate player two
            PlayerTwo.GetComponent<Rigidbody>().mass = PlayerTwo.transform.localScale.x;
            PlayerTwo.GetComponent<Rigidbody>().useGravity = true;
            PlayerTwo.GetComponent<PlayerBlow>().enabled = false;
            PlayerTwo.GetComponent<PlayerController>().enabled = true;

            // change game stage to fighting stage
            CurrentStage = GameStage.Fighting;
            timer = 0.9F;
            TopText.rectTransform.anchorMin = new Vector2(0.5F, 0.5F);
            TopText.rectTransform.anchorMax = new Vector2(0.5F, 0.5F);
            TopText.rectTransform.pivot = new Vector2(0.5F, 0.5F);
            TopText.fontSize = 48;
            TopText.text = "FIGHT!";
            audioSource.PlayOneShot(announcer_fight, 1.0f);
        }
        PlayerOne.GetComponentInChildren<Text>().fontSize = 32;
        PlayerTwo.GetComponentInChildren<Text>().fontSize = 32;
    }

    /// <summary>
    /// This function is called every frame in the fighting stage.</summary>
    private void UpdateFighting() {
        timer -= Time.deltaTime;
        if (timer <= 0F)
        {
            TopText.fontSize = 24;
            TopText.rectTransform.anchorMin = new Vector2(0.5F, 0.97F);
            TopText.rectTransform.anchorMax = new Vector2(0.5F, 0.97F);
            TopText.rectTransform.pivot = new Vector2(0.5F, 1F);
            TopText.text = "<color=red>P1: </color>" + PlayerOneScore + "\t\t\t<color=yellow>P2: </color>" + PlayerTwoScore;

            // display winner message
            if (PlayerOneScore >= WinScore || PlayerTwoScore >= WinScore)
            {
                timer = 5F;
                TopText.fontSize = 32;
                TopText.rectTransform.anchorMin = new Vector2(0.5F, 0.5F);
                TopText.rectTransform.anchorMax = new Vector2(0.5F, 0.5F);
                TopText.rectTransform.pivot = new Vector2(0.5F, 0.5F);
                TopText.text = (PlayerOneScore >= WinScore ? "<color=red>Player One Wins!</color>" : "<color=yellow>Player Two Wins!</color>");
                CurrentStage = GameStage.PostFighting;
            }
        }
    }

    /// <summary>
    /// This function is called every frame in the post fighting stage.</summary>
    private void UpdatePostFighting() {
        timer -= Time.deltaTime;

        // go back to level select
        if (timer <= 0F) {
            SceneManager.LoadScene("PreGameLocal");
        }
    }

    private void hideButtonBlowControls(bool enable) {
        PlayerOneBtn1.gameObject.SetActive(enable);
        PlayerOneBtn2.gameObject.SetActive(enable);
    }
}
