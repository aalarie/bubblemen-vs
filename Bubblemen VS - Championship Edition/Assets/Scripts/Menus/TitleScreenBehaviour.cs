using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenBehaviour : MonoBehaviour 
{
    public Image gameLogo;
    public Text pressEnterText;
    public Text copyrightText;
    public GameObject modeSelectButtons;
    public GameObject tourneyButton, multiplayerButton, onlineButton;
    public AudioSource audioSource;
    public AudioClip announcerTitle, titleBGM;

    private float tourneyButtonDefaultY, multiplayerButtonDefaultY, onlineButtonDefaultY, angle = 0;

    private bool introHasBeenStarted = false, introHasBeenPlayed = false;
    private bool animatePressEnter = false, pressEnterFadeIn = true;
    private bool animateGameModeButtons = false;

	// Use this for initialization
	void Start () 
    {
        // Logo & copyright are invisble
        gameLogo.color = new Color(gameLogo.color.r, gameLogo.color.g, gameLogo.color.b, 0);
        copyrightText.color = new Color(copyrightText.color.r, copyrightText.color.g, copyrightText.color.b, 0);

        // Press Enter text hidden
        pressEnterText.color = new Color(pressEnterText.color.r, pressEnterText.color.g, pressEnterText.color.b, 0);

        // Mode Select Buttons are hidden
        tourneyButtonDefaultY = tourneyButton.transform.position.y;
        multiplayerButtonDefaultY = tourneyButton.transform.position.y;
        onlineButtonDefaultY = tourneyButton.transform.position.y;
        modeSelectButtons.SetActive(false);

        // Music not playing
        audioSource.Stop();
	}
	
	// Update is called once per frame
	void Update () 
    {
        // If logo & copyright are not fully displayed yet, continue animating it
        if (gameLogo.color.a < 1.0f)
        {
            float newAlphaValue = gameLogo.color.a + Time.deltaTime * 0.6f;
            gameLogo.color = new Color(gameLogo.color.r, gameLogo.color.g, gameLogo.color.b, newAlphaValue);
            copyrightText.color = new Color(copyrightText.color.r, copyrightText.color.g, copyrightText.color.b, newAlphaValue);
        }
        // Once the logo has finished animating, play announcer voice and show press enter
        else if (!introHasBeenStarted)
        {
            StartCoroutine(playIntroTransition());
            introHasBeenStarted = true; // Don't do this transition again
        }

        if (animatePressEnter)
        {
            if (pressEnterFadeIn)
            {
                float newAlphaValue = pressEnterText.color.a + Time.deltaTime * 0.5f;
                pressEnterText.color = new Color(pressEnterText.color.r, pressEnterText.color.g, pressEnterText.color.b, newAlphaValue);
                if (pressEnterText.color.a >= 1.0f)
                    pressEnterFadeIn = false;
            }
            else
            {
                float newAlphaValue = pressEnterText.color.a - Time.deltaTime * 0.5f;
                pressEnterText.color = new Color(pressEnterText.color.r, pressEnterText.color.g, pressEnterText.color.b, newAlphaValue);
                if (pressEnterText.color.a <= 0.0f)
                    pressEnterFadeIn = true;
            }
        }

        // If user presses enter, hide press enter text and show game selection options
        if (introHasBeenPlayed && Input.GetKeyDown(KeyCode.Return))
        {
            // Stop animating press enter and hide the text
            animatePressEnter = false;
            pressEnterText.gameObject.SetActive(false);

            // Show mode select buttons
            modeSelectButtons.SetActive(true);

            // Start animating game mode buttons
            animateGameModeButtons = true;
        }

        if (animateGameModeButtons)
        {
            Vector3 tourneyButtonPos = tourneyButton.transform.position;
            Vector3 multiplayerButtonPos = multiplayerButton.transform.position;
            Vector3 onlineButtonPos = onlineButton.transform.position;

            // Make bubble buttons float up and down
            angle += 50 * Time.deltaTime;
            if (angle > 360)
                angle -= 360;
            tourneyButton.transform.position = new Vector3(tourneyButtonPos.x, tourneyButtonDefaultY + 2.0f * Mathf.Sin(angle * Mathf.PI/180), tourneyButtonPos.z);
            multiplayerButton.transform.position = new Vector3(multiplayerButtonPos.x, multiplayerButtonDefaultY + 2.2f * Mathf.Sin(angle * Mathf.PI/180), multiplayerButtonPos.z);
            onlineButton.transform.position = new Vector3(onlineButtonPos.x, onlineButtonDefaultY + 1.8f * Mathf.Sin(angle * Mathf.PI/180), onlineButtonPos.z);
        }

    }

    IEnumerator playIntroTransition()
    {
        // Play announcer
        audioSource.PlayOneShot(announcerTitle, 1.0f);

        // Once announcer is done play bgm and display press enter
        yield return new WaitForSeconds(announcerTitle.length);
        audioSource.clip = titleBGM;
        audioSource.Play();
        animatePressEnter = true;
        introHasBeenPlayed = true;
    }
}
