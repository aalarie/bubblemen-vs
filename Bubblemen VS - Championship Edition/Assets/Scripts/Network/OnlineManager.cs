using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

/// <summary>
/// Handles all online elements of the game.</summary>
public class OnlineManager : NetworkLobbyManager {

    /// <summary>
    /// Player 1's bubbleman.</summary>
    public GameObject PlayerOne;

    /// <summary>
    /// Player 2's bubbleman.</summary>
    public GameObject PlayerTwo;

    /// <summary>
    /// The number of players who are ready.</summary>
    private int numPlayersReady = 0;

    /// <summary>
    /// Whether the client is attempting joining a match</summary>
    private bool isJoiningMatch = false;

    /// <summary>
    /// Text element to show status feedback in pre-game scene.</summary>
    private Text preGameStatusText;

    /// <summary>
    /// This function is called on the client when disconnected from a server.</summary>
    /// <param name="conn">The connection that disconnected.</param>
    public override void OnLobbyClientDisconnect(NetworkConnection conn) {
        if (isJoiningMatch && preGameStatusText != null) {
            // retry again if failed to join match
            preGameStatusText.text = "Error connecting.\nSearching for new match...";
            matchMaker.ListMatches(0, 20, "", false, 0, 0, JoinMatchOnMatchList);
        }

        base.OnLobbyClientDisconnect(conn);
    }

    /// <summary>
    /// This function is called when a <c>NetworkMatch.CreateMatch</c> request has been processed on the server.</summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">A text description for the error if success is false.</param>
    /// <param name="matchInfo">The information about the newly created match.</param>
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo) {
        // upon create of a match, reset the number of player who are ready
        if (success) {
            numPlayersReady = 0;
        }

        base.OnMatchCreate(success, extendedInfo, matchInfo);
    }

    /// <summary>
    /// This function is called on the server when a client is ready.</summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnection conn) {
        if (conn.playerControllers.Count == 1) {
            // increment when player count when they are ready
            numPlayersReady++;

            // assign player as player one or player two
            if (PlayerOne == null) {
                PlayerOne = conn.playerControllers[0].gameObject;
            } else {
                PlayerTwo = conn.playerControllers[0].gameObject;
            }
        }

        // once two players are ready, start the game
        if (numPlayersReady == 2) {
            GameObject.Find("Game Manager").GetComponent<NetworkGameManager>().StartPreBubbleBlowingStage();
        }

        base.OnServerReady(conn);
    }

    /// <summary>
    /// Creates a room for a match ands waits for one other player.</summary>
    /// <param name="statusText">Text element to display status messages.</param>
    public void HostMatch(Text statusText) {
        preGameStatusText = statusText;

        // start the matchmaking service
        StartMatchMaker();
        preGameStatusText.text = "Waiting for other player...";

        // get a new match lobby
        matchName = "BUBBLEMEN-VS-ONLINE-MATCH";
        matchMaker.CreateMatch(matchName, matchSize, true, "", "", "", 0, 0, OnMatchCreate);
    }

    /// <summary>
    /// Cancels the hosting of a multiplayer match.</summary>
    public void CancelHostMatch() {
        // stop hosting a match
        StopHost();
    }

    /// <summary>
    /// Waits for an open multiplayer room to become available and attemps to join it.</summary>
    /// <param name="statusText">Text element to display status messages.</param>
    public void JoinMatch(Text statusText) {
        preGameStatusText = statusText;

        // start the matchmaking service
        StartMatchMaker();
        preGameStatusText.text = "Searching for a match...";

        // get a list of all available matches and try to join a match
        matchMaker.ListMatches(0, 20, "", false, 0, 0, JoinMatchOnMatchList);
    }


    /// <summary>
    /// Cancels attempting to join a match</summary>
    public void CancelJoinMatch() {
        // stops the matchmaker
        StopMatchMaker();
    }

    /// <summary>
    /// Once a list of available matches is created, try to join the first one.</summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">A text description for the error if success is false.</param>
    /// <param name="matchList">A list of matches corresponding to the filters set in the initial list request.</param>
    private void JoinMatchOnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList) {
        isJoiningMatch = false;

        // list the available matches
        OnMatchList(success, extendedInfo, matchList);

        // if there is a match available, try to join it
        if (matches.Count > 0) {
            matchName = matches[matches.Count - 1].name;
            matchSize = (uint) matches[matches.Count - 1].currentSize;

            isJoiningMatch = true;
            preGameStatusText.text = "Match found. Loading...";
            matchMaker.JoinMatch(matches[matches.Count - 1].networkId, "", "", "", 0, 0, OnMatchJoined);
        } else {
            // refresh the match list and try again
            matchMaker.ListMatches(0, 20, "", false, 0, 0, JoinMatchOnMatchList);
        }
    }
}
