// Based on: http://answers.unity3d.com/questions/645114/how-to-make-a-smash-bros-like-camera.html
using UnityEngine;

public class DynamicFightCamera : MonoBehaviour
{
    private float minX, maxX, minY, maxY;
    private Vector3 finalLookAt;
    private Camera camera;
    private GameObject[] players;
    private float originalPlayerDist, previousPlayerDist;

    public float camSpeed, camDist;
    public Vector3 angles;
    public float cameraHeight;

    // Use this for initialization
    private void Start()
    {
        camera = GetComponent<Camera>();
        finalLookAt = new Vector3(0.0f, 0.0f, 0.0f);

        players = GameObject.FindGameObjectsWithTag("Player");

        // Have to hardcode this because of online
        //float cameraBuffer = 5.0f;
        originalPlayerDist =  11f; //Mathf.Abs(players[0].transform.position.x - players[1].transform.position.x) + cameraBuffer;
    }

    // Update is called once per frame
    private void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 0) {
            CalculateBounds();
            CalculateCameraPosAndSize();
        }
    }

    private void CalculateBounds()
    {
        minX = Mathf.Infinity;
        maxX = -Mathf.Infinity;
        minY = Mathf.Infinity;
        maxY = -Mathf.Infinity;

        foreach (GameObject player in players) {
            Vector3 tempPlayer = player.transform.position;

            //X Bounds
            if (tempPlayer.x < minX)
                minX = tempPlayer.x;
            if (tempPlayer.x > maxX)
                maxX = tempPlayer.x;

            //Y Bounds
            if (tempPlayer.y < minY)
                minY = tempPlayer.y;
            if (tempPlayer.y > maxY)
                maxY = tempPlayer.y;
        }
    }

    private void CalculateCameraPosAndSize()
    {
        // Position Vector3 
        Vector3 cameraCenter = Vector3.zero;

        foreach (GameObject player in players) {
            cameraCenter += player.transform.position;
        }

        Vector3 finalCameraCenter = cameraCenter / players.Length;
        finalCameraCenter.y += cameraHeight; // Bias as camera is always looking slightly down

        // Determine if we require panning
        if (players.Length > 1)
        {
            // If a player is out of view, we need to pan out
            if (!isPlayerViewable(players[0]) || !isPlayerViewable(players[1]))
                camDist += 0.1f;
            // If a both players have returned to 
            else
            {
                Vector3 player1ScreenPos = camera.WorldToScreenPoint(players[0].transform.position);
                Vector3 player2ScreenPos = camera.WorldToScreenPoint(players[1].transform.position);
                float currentPlayerDist = Mathf.Abs(players[0].transform.position.x - players[1].transform.position.x);

                // If players have come closer we need to pan in, unless smaller than original viewport size
                if (currentPlayerDist < previousPlayerDist && currentPlayerDist > originalPlayerDist)
                {
                    camDist -= 0.1f;
                }

                // Save as previous distance
                previousPlayerDist = currentPlayerDist;
            }
        }

        // Rotates and Positions camera around a point
        var rot = Quaternion.Euler(angles);
        var pos = rot * new Vector3(0f, 0f, camDist) + finalCameraCenter;
        transform.rotation = rot;
        transform.position = Vector3.Lerp(transform.position, pos, camSpeed * Time.deltaTime);
        finalLookAt = Vector3.Lerp(finalLookAt, finalCameraCenter, camSpeed * Time.deltaTime);
        transform.LookAt(finalLookAt);
    }

    private bool isPlayerViewable(GameObject player)
    {
        Vector3 playerScreenPos = camera.WorldToScreenPoint(player.transform.position);

        // 15 pixel buffer in the x direction
        if (playerScreenPos.x - 40 < 0 || playerScreenPos.x + 40 > Screen.width)
            return false;
        else
            return true;
    }
}
