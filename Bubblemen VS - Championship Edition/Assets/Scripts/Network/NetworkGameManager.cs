using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Handles networked game logic for Bubblemen VS.</summary>
public class NetworkGameManager : NetworkBehaviour {
    /// <summary>
    /// Bubblemen VS game stages.</summary>
    public enum GameStage {

        /// <summary>
        /// The game is not ready yet.</summary>
        Loading,

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
    /// Text element located at the top of the screen.</summary>
    public Text TopText;

    /// <summary>
    /// Escape menu panel.</summary>
    public GameObject EscapePanel;

    /// <summary>
    /// Audio source of announcer clips.</summary>
    public AudioSource Source;

    /// <summary>
    /// List of announcer audio clips.</summary>
    public List<AudioClip> Announcer;

    /// <summary>
    /// Left blow button picture.</summary>
    public GameObject LeftBlowButton;

    /// <summary>
    /// Right blow button picture.</summary>
    public GameObject RightBlowButton;

    /// <summary>
    /// The current stage of the game.</summary>
    public GameStage CurrentStage = GameStage.Loading;

    /// <summary>
    /// Player 1's score.</summary>
    [SyncVar]
    public int PlayerOneScore = 0;

    /// <summary>
    /// Player 2's score.</summary>
    [SyncVar]
    public int PlayerTwoScore = 0;

    /// <summary>
    /// The score to reach to win the game.</summary>
    public int WinScore = 5;

    /// <summary>
    /// Game timer.</summary>
    private float timer;

    /// <summary>
    /// Commence FightScene so remove text on Screen.</summary>
    private bool removeText;

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
    /// The networking manager.</summary>
    private OnlineManager om;

    /// <summary>
    /// This function is called on the frame when a script is enabled just before any of the Update methods is called the first time.</summary>
    private void Start() {
        if (!isServer) {
            return;
        }

        om = GameObject.Find("Online Manager").GetComponent<OnlineManager>();
    }

    /// <summary>
    /// This function is called every frame, if the <c>MonoBehaviour</c> is enabled.</summary>
    private void Update() {
        if (!isServer) {
            return;
        }

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
    }

    /// <summary>
    /// Tells the game manager to start the pre-bubble blowing stage.</summary>
    [Server]
    public void StartPreBubbleBlowingStage() {
        // start game off in the bubble blowing stage
        CurrentStage = GameStage.PreBubbleBlowing;
        timer = 5F;
        countdownSecs = 1F;
        countdown = 4;
        blowKeys = 0.5F;
        alterateKeys = false;
    }

    /// <summary>
    /// This function is called every frame in the pre blow up stage.</summary>
    [Server]
    private void UpdateBlowingPhaseCountdown() {
        // this will allow for a 1 second delay between each number in the countdown
        countdownSecs -= Time.deltaTime;
        RpcSetTopTextPosition(new Vector2(0.5F, 0.5F), new Vector2(0.5F, 0.5F), new Vector2(0.5F, 0.5F));

        if (countdownSecs > 0) {
            if (countdown == 4) {
                RpcUpdateTopText("");
            } else if (countdown > 0) {
                RpcUpdateTopText("" + countdown);
            }
        } else {
            countdown--;
            countdownSecs = 1F;

            // print BLOW! if we have reached the end of the countdown
            if (countdown == 3) {
                RpcPlayAnnouncerClip(0);
            } else if (countdown == 2) {
                RpcPlayAnnouncerClip(1);
            } else if (countdown == 1) {
                RpcPlayAnnouncerClip(2);
            } else if (countdown == 0) {
                RpcUpdateTopText("BLOW!");
                RpcPlayAnnouncerClip(3);
            }

            // this is to allow for a 1 sec delay of the text BLOW! before the start of the timer
            else if (countdown == -1) {
                RpcActivatePlayersForBubbleBlowing(om.PlayerOne, om.PlayerTwo);
                CurrentStage = GameStage.BubbleBlowing;
                RpcHideButtonBlowControls(true);
            }
        }
    }

    /// <summary>
    /// This function is called every frame in the bubble blowing stage.</summary>
    [Server]
    private void UpdateBubbleBlowing() {
        // decrement bubble blowing timer
        timer -= Time.deltaTime;
        blowKeys -= Time.deltaTime;
        RpcSetTopTextPosition(new Vector2(0.5F, 0.98F), new Vector2(0.5F, 0.98F), new Vector2(0.5F, 0.98F));

        if (timer > 0) {
            // update clock
            RpcUpdateTopText("");

            if (blowKeys > 0 && !alterateKeys) {
                RpcButtonSetLeftColoured(true);
            }
            if (blowKeys < 0) {
                alterateKeys = alterateKeys ? false : true;
                RpcButtonSetLeftColoured(false);
                blowKeys = 0.5F;
            }
        } else {
            RpcUpdateTopText("Time is up!");
            RpcHideButtonBlowControls(false);

            // activate players
            RpcActivatePlayersForFighting(om.PlayerOne, om.PlayerTwo);

            // change game stage to fighting stage
            CurrentStage = GameStage.Fighting;
            timer = 0.9F;
            RpcSetTopTextPosition(new Vector2(0.5F, 0.5F), new Vector2(0.5F, 0.5F), new Vector2(0.5F, 0.5F));
            RpcUpdateTopTextPlus("FIGHT!", 48);
            RpcPlayAnnouncerClip(4);
            removeText = true;
        }
    }

    /// <summary>
    /// This function is called every frame in the bubble blowing stage.</summary>
    [Server]
    public void UpdateFighting() {
        timer -= Time.deltaTime;
        if (timer <= 0F) {
            RpcDisplayGameScore(om.PlayerOne, PlayerOneScore, PlayerTwoScore);

            // display winner message
            if (PlayerOneScore >= WinScore || PlayerTwoScore >= WinScore) {
                timer = 5F;

                if (PlayerOneScore >= WinScore) {
                    RpcDisplayPostGameText(om.PlayerOne);
                } else {
                    RpcDisplayPostGameText(om.PlayerTwo);
                }
                CurrentStage = GameStage.PostFighting;
            }
        }
    }

    /// <summary>
    /// This function is called every frame in the bubble blowing stage.</summary>
    [Server]
    public void UpdatePostFighting() {
        timer -= Time.deltaTime;

        // disconnect players and close match
        if (timer <= 0F) {
            NetworkServer.Shutdown();
            om.CancelHostMatch();
        }
    }

    /// <summary>
    /// Plays the announcer audio clip at specified index.</summary>
    /// <param name="index">Index of the clip.</param>
    [ClientRpc]
    private void RpcPlayAnnouncerClip(int index) {
        Source.PlayOneShot(Announcer[index], 1.0F);
    }

    /// <summary>
    /// Tells all clients to appropriately update the text at the top of the screen.</summary>
    /// <param name="text">The updated text.</param>
    [ClientRpc]
    private void RpcUpdateTopText(string text) {
        Text topText = GameObject.Find("Canvas/Top Text").GetComponent<Text>();
        topText.text = text;
    }

    /// <summary>
    /// Tells all clients to appropriately update the text at the top of the screen.</summary>
    /// <param name="text">The updated text.</param>
    /// <param name="fontSize">The size of the font for the text.</param>
    [ClientRpc]
    private void RpcUpdateTopTextPlus(string text, int fontSize) {
        Text topText = GameObject.Find("Canvas/Top Text").GetComponent<Text>();
        topText.text = text;
        topText.fontSize = fontSize;
    }

    /// <summary>
    /// Tells all clients to move the position of the top text.</summary>
    /// <param name="anchorMin">The lower-left anchor.</param>
    /// <param name="anchorMax">The upper-right anchor.</param>
    /// <param name="pivot">The position around which it rotates.</param>
    [ClientRpc]
    private void RpcSetTopTextPosition(Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot) {
        Text topText = GameObject.Find("Canvas/Top Text").GetComponent<Text>();
        topText.rectTransform.anchorMin = anchorMin;
        topText.rectTransform.anchorMax = anchorMax;
        topText.rectTransform.pivot = pivot;
    }

    /// <summary>
    /// Tells all clients to enable/disable the button blow controls.</summary>
    /// <param name="enable">Whether to enable the controls graphic.</param>
    [ClientRpc]
    private void RpcHideButtonBlowControls(bool enable) {
        LeftBlowButton.SetActive(enable);
        RightBlowButton.SetActive(enable);
    }

    /// <summary>
    /// Tells all clients to colour the left or right blow key cyan and the other key white.</summary>
    /// <param name="isColoured">Is the left key the one that's coloured?</param>
    [ClientRpc]
    private void RpcButtonSetLeftColoured(bool isColoured) {
        LeftBlowButton.GetComponent<Image>().color = isColoured ? Color.cyan : Color.white;
        RightBlowButton.GetComponent<Image>().color = isColoured ? Color.white : Color.cyan;
    }

    /// <summary>
    /// Tells all clients to move players to the bubble blowing stage.</summary>
    /// <param name="playerOne">Player 1's bubbleman.</param>
    /// <param name="playerTwo">Player 2's bubbleman.</param>
    [ClientRpc]
    private void RpcActivatePlayersForBubbleBlowing(GameObject playerOne, GameObject playerTwo) {
        playerOne.name = "Player 1";
        playerOne.GetComponent<NetworkPlayerBlow>().enabled = true;
        playerTwo.name = "Player 2";
        playerTwo.GetComponent<NetworkPlayerBlow>().enabled = true;
    }

    /// <summary>
    /// Tells all clients to move players to the fighting stage.</summary>
    /// <param name="playerOne">Player 1's bubbleman.</param>
    /// <param name="playerTwo">Player 2's bubbleman.</param>
    [ClientRpc]
    private void RpcActivatePlayersForFighting(GameObject playerOne, GameObject playerTwo) {
        // activate player one
        playerOne.GetComponent<Rigidbody>().mass = playerOne.transform.localScale.x;
        playerOne.GetComponent<Rigidbody>().useGravity = true;
        playerOne.GetComponent<NetworkPlayerBlow>().enabled = false;
        playerOne.GetComponent<NetworkPlayerController>().enabled = true;

        // activate player two
        playerTwo.GetComponent<Rigidbody>().mass = playerTwo.transform.localScale.x;
        playerTwo.GetComponent<Rigidbody>().useGravity = true;
        playerTwo.GetComponent<NetworkPlayerBlow>().enabled = false;
        playerTwo.GetComponent<NetworkPlayerController>().enabled = true;
    }

    /// <summary>
    /// Show the appropriate game score on all clients.</summary>
    /// <param name="playerOne">Player 1's game object</param>
    /// <param name="playerOneScore">Player 1's score.</param>
    /// <param name="playerTwoScore">Player 2's score.</param>
    [ClientRpc]
    private void RpcDisplayGameScore(GameObject playerOne, int playerOneScore, int playerTwoScore) {
        if (playerOne == null) {
            return;
        }

        Text topText = GameObject.Find("Canvas/Top Text").GetComponent<Text>();
        topText.fontSize = 24;
        topText.rectTransform.anchorMin = new Vector2(0.5F, 0.97F);
        topText.rectTransform.anchorMax = new Vector2(0.5F, 0.97F);
        topText.rectTransform.pivot = new Vector2(0.5F, 1F);

        if (playerOne.GetComponent<NetworkIdentity>().isLocalPlayer) {
            topText.text = "<color=red>You: </color>" + playerOneScore + "\t\t\t<color=yellow>Opponent: </color>" + playerTwoScore;
        } else {
            topText.text = "<color=yellow>You: </color>" + playerTwoScore + "\t\t\t<color=red>Opponent: </color>" + playerOneScore;
        }
    }

    /// <summary>
    /// Show the appropriate post-game message on all clients.</summary>
    /// <param name="winner">The winning bubbleman.</param>
    [ClientRpc]
    private void RpcDisplayPostGameText(GameObject winner) {
        Text topText = GameObject.Find("Canvas/Top Text").GetComponent<Text>();
        topText.fontSize = 32;
        topText.rectTransform.anchorMin = new Vector2(0.5F, 0.5F);
        topText.rectTransform.anchorMax = new Vector2(0.5F, 0.5F);
        topText.rectTransform.pivot = new Vector2(0.5F, 0.5F);
        topText.text = winner.GetComponent<NetworkIdentity>().isLocalPlayer ? "You Win!" : "You Lose!";
    }
}
