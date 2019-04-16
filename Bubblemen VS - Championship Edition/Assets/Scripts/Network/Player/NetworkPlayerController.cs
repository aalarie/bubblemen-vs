using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class for handling networked player behaviour in the fighting stage.</summary>
public class NetworkPlayerController : NetworkBehaviour {

    /// <summary>
    /// Struct to hold colliders used for attacking.</summary>
    [System.Serializable]
    public struct AttackColliders {

        /// <summary>
        /// The collider that is on the player's left fist.</summary>
        public Collider Punch;

        /// <summary>
        /// The collider that is on the player's feet.</summary>
        public Collider Kick;
    };

    /// <summary>
    /// Struct to hold axis names for user input.</summary>
    [System.Serializable]
    public struct Axes {

        /// <summary>
        /// Movement axis name.</summary>
        public string Move;

        /// <summary>
        /// Jump axis name.</summary>
        public string Jump;

        /// <summary>
        /// Punch axis name.</summary>
        public string Punch;
    };

    /// <summary>
    /// A list of player sound effects.</summary>
    public List<AudioClip> PlayerSFX;

    /// <summary>
    /// Location where the player respawns.</summary>
    public Vector3 RespawnPoint;

    [Header("Movement")]
    /// <summary>
    /// Axes for user input.</summary>
    public Axes InputAxes;

    /// <summary>
    /// Player movement speed.</summary>
    public float Speed;

    /// <summary>
    /// Player jump strength.</summary>
    public float JumpStrength;

    /// <summary>
    /// Height above the ground at which the player is considered to be airborne.</summary>
    public float AirborneHeight;

    [Header("Combat")]
    /// <summary>
    /// The attack colliders on the player's character.</summary>
    public AttackColliders Colliders;

    /// <summary>
    /// The knockback strength of the punch attack.</summary>
    public float PunchStrength;

    /// <summary>
    /// The knockback strength of the kick attack.</summary>
    public float KickStrength;

    /// <summary>
    /// The magnitude of impulse required to pop the player.</summary>
    public float PopThreshold;

    /// <summary>
    /// The timer of how long the player is vulnerable to being popped.</summary>
    public float PopVulnerableTimer;

    /// <summary>
    /// The timer of how long the player is invulnerable after respawning.</summary>
    public float RespawnInulnerableTimer;

    /// <summary>
    /// Whether the player is currently in an attack animation.</summary>
    public bool IsAttacking = false;

    /// <summary>
    /// Whether the player is punching.</summary>
    public bool IsPunching = false;

    /// <summary>
    /// The height of the player's collider.</summary>
    private float playerHeight;

    /// <summary>
    /// The radius of the player's collider.</summary>
    private float playerRadius;

    /// <summary>
    /// Player object's animator.</summary>
    private Animator anim;

    /// <summary>
    /// Player object's rigidbody.</summary>
    private Rigidbody rb;

    /// <summary>
    /// The game manager.</summary>
    private NetworkGameManager gm;

    /// <summary>
    /// The attack collider which is currently enabled.</summary>
    private Collider activeAttackCollider;

    private AudioSource AudioSourceSFX;

    private AudioSource AudioSourceFootsteps;

    /// <summary>
    /// This function is called on the frame when a script is enabled just before any of the Update methods is called the first time.</summary>
    private void Start() {
        // manage audio sources
        AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
        AudioSourceSFX = audioSources[0];
        AudioSourceFootsteps = audioSources[1];
        AudioSourceFootsteps.clip = PlayerSFX[0];
        AudioSourceFootsteps.loop = true;

        gm = GameObject.Find("Game Manager").GetComponent<NetworkGameManager>();
        RespawnPoint = transform.position + 3F * transform.up;

        // get components on player
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // find the height of the player
        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        playerRadius = GetComponent<CapsuleCollider>().radius * transform.localScale.x;

        // determine the pop threshold for the player based on their size
        PopThreshold = 3F * Mathf.Sin(transform.localScale.y) - 4F * Mathf.Cos(transform.localScale.y) + 7F;
    }

    /// <summary>
    /// This function is called every frame, if the <c>MonoBehaviour</c> is enabled.</summary>
    private void Update() {
        if (!isLocalPlayer) {
            return;
        }

        // decrement pop vulnerability timer
        PopVulnerableTimer -= Time.deltaTime;
        if (PopVulnerableTimer < 0F) {
            PopVulnerableTimer = 0F;
        }
        RespawnInulnerableTimer -= Time.deltaTime;
        if (RespawnInulnerableTimer < 0F) {
            RespawnInulnerableTimer = 0F;
        }

        // prevent queued kicking
        if (IsPlayerAirborne()) {
            anim.ResetTrigger("Kick");
        }

        GetInput();
        Move();

        // if airborne, stop footstep noises that are playing
        if (AudioSourceFootsteps.isPlaying && IsPlayerAirborne()) {
            CmdPauseFootstepsClip(gameObject);
        }

        // Listen for escape key presses from this player
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (gm.EscapePanel.activeSelf == true) {
                gm.EscapePanel.SetActive(false);
            } else {
                gm.EscapePanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// This function is called when this collider/rigidbody has begun touching another rigidbody/collider.</summary>
    /// <param name="collision">The Collision data associated with this collision.</param>
    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Player") {
            // if we collide with other player, play bump sound locally
            AudioSourceSFX.PlayOneShot(PlayerSFX[4], 0.5F);
        }

        if (!isLocalPlayer) {
            return;
        }

        // pop the player if they hit a surface too hard
        if (RespawnInulnerableTimer == 0F && PopVulnerableTimer > 0F && Vector3.Magnitude(collision.impulse) >= PopThreshold) {
            PopPlayer(gameObject);
        }
    }

    /// <summary>
    /// Returns whether the player is up against a wall.</summary>
    /// <returns>Returns true is the player is against a wall, false otherwise.</returns>
    public bool IsPlayerAgainstWall() {
        return Physics.Raycast(transform.position + transform.up * (playerHeight / 4), transform.forward, playerRadius + 0.05F, ~(1 << LayerMask.NameToLayer("Player"))) ||
            Physics.Raycast(transform.position + transform.up * (playerHeight / 4), -transform.forward, playerRadius + 0.05F, ~(1 << LayerMask.NameToLayer("Player")));
    }

    /// <summary>
    /// Pops the player and increment the other's score.</summary>
    ///<param name="player">The player to pop.</param>
    public void PopPlayer(GameObject player) {
        // play pop sound
        CmdPlaySFXClip(player.gameObject, 3, 1F);

        if (player.name == "Player 1") {
            if (gm.PlayerTwoScore < gm.WinScore - 1) {
                // increment player 2's score
                CmdIncrementPlayerTwoScore();

                // respawn player is game continues
                CmdRespawnPlayer(player.gameObject, player.GetComponentInParent<NetworkPlayerController>().RespawnPoint);
            } else {
                // increment player 2's score
                CmdIncrementPlayerTwoScore();

                CmdPopPlayer(player.gameObject);
            }
        } else {
            if (gm.PlayerOneScore < gm.WinScore - 1) {
                // increment player 1's score
                CmdIncrementPlayerOneScore();

                // respawn player is game continues
                CmdRespawnPlayer(player.gameObject, player.GetComponentInParent<NetworkPlayerController>().RespawnPoint);
            } else {
                // increment player 1's score
                CmdIncrementPlayerOneScore();

                CmdPopPlayer(player.gameObject);
            }
        }
    }

    /// <summary>
    /// Turns off the currently active attack collider.</summary>
    public void ResetAttackColliderEnabled() {
        // disable attack collider(s)
        if (IsPunching) {
            Colliders.Punch.enabled = false;
        } else {
            Colliders.Kick.enabled = false;
        }

        // reset punching flag
        IsPunching = false;
    }

    /// <summary>
    /// This function is called by the player's attack collider when they hit the enemy.</summary>
    /// <param name="other">The enemy bubbleman.</param>
    /// <param name="isPunch">Whether the attack is a punch.</param>
    public void OnAttackCollided(GameObject other, bool isPunch) {
        // pop player if they are against a wall
        if (RespawnInulnerableTimer == 0F && other.GetComponentInParent<NetworkPlayerController>().IsPlayerAgainstWall()) {
            PopPlayer(other);
            return;
        }

        // get the player's knockback strength
        float strength;
        if (isPunch) {
            strength = GetComponentInParent<NetworkPlayerController>().PunchStrength;
        } else {
            strength = GetComponentInParent<NetworkPlayerController>().KickStrength;
        }

        // calculate the strength of the attack
        float sourceMass = GetComponentInParent<Rigidbody>().mass;
        float targetMass = other.GetComponentInParent<Rigidbody>().mass;
        float hitForce = sourceMass * (1 / targetMass) * strength;
        Debug.LogFormat("{0} hits {1} with {2} force", transform.root.name, other.transform.root.name, hitForce);

        // knock the other player backwards
        CmdApplyKnockback(other, transform.root.forward * hitForce);

        // play bump sound
        CmdPlaySFXClip(gameObject, 4, 0.5F);

        other.GetComponent<Animator>().Play("get_hit");
    }

    /// <summary>
    /// Tells the server to increment player 1's score.</summary>
    [Command]
    private void CmdIncrementPlayerOneScore() {
        GameObject.Find("Game Manager").GetComponent<NetworkGameManager>().PlayerOneScore++;
    }

    /// <summary>
    /// Tells the server to increment player 2's score.</summary>
    [Command]
    private void CmdIncrementPlayerTwoScore() {
        GameObject.Find("Game Manager").GetComponent<NetworkGameManager>().PlayerTwoScore++;
    }

    /// <summary>
    /// Tells the server to respawn a bubbleman.</summary>
    /// <param name="player">The bubbleman to respawn.</param>
    /// <param name="respawnPoint">The point at which to respawn the bubbleman.</param>
    [Command]
    private void CmdRespawnPlayer(GameObject player, Vector3 respawnPoint) {
        RpcRespawnPlayer(player, respawnPoint);
    }

    /// <summary>
    /// Tells all clients to respawn a bubbleman.</summary>
    /// <param name="player">The bubbleman to respawn.</param>
    /// <param name="respawnPoint">The point at which to respawn the bubbleman.</param>
    [ClientRpc]
    private void RpcRespawnPlayer(GameObject player, Vector3 respawnPoint) {
        player.transform.position = respawnPoint;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.GetComponent<NetworkPlayerController>().PopVulnerableTimer = 0F;
        player.GetComponent<NetworkPlayerController>().RespawnInulnerableTimer = 0.5F;
    }

    /// <summary>
    /// Tells the server to pop a bubbleman.</summary>
    /// <param name="player">The bubbleman to pop.</param>
    [Command]
    private void CmdPopPlayer(GameObject player) {
        NetworkServer.Destroy(player);
    }

    /// <summary>
    /// Tells the server to play a player SFX audio clip.</summary>
    /// <param name="player">The player on which to play the SFX.</param>
    /// <param name="index">The index of the audio clip in the SFX list.</param>
    /// <param name="volumeScale">The volume of the clip.</param>
    [Command]
    public void CmdPlaySFXClip(GameObject player, int index, float volumeScale) {
        RpcPlaySFXClip(player, index, volumeScale);
    }

    /// <summary>
    /// Tells all clients to play a player SFX audio clip.</summary>
    /// <param name="player">The player on which to play the SFX.</param>
    /// <param name="index">The index of the audio clip in the SFX list.</param>
    /// <param name="volumeScale">The volume of the clip.</param>
    [ClientRpc]
    public void RpcPlaySFXClip(GameObject player, int index, float volumeScale) {
        player.GetComponentInParent<NetworkPlayerController>().AudioSourceSFX.PlayOneShot(PlayerSFX[index], volumeScale);
    }

    /// <summary>
    /// Tells the server to play the footsteps audio clip for a player.</summary>
    /// <param name="player">The player on which to player footsteps SFX.</param>
    [Command]
    public void CmdPlayFootstepsClip(GameObject player) {
        RpcPlayFootstepsClip(player);
    }

    /// <summary>
    /// Tells all clients to play the footsteps audio clip for a player.</summary>
    /// <param name="player">The player on which to player footsteps SFX.</param>
    [ClientRpc]
    public void RpcPlayFootstepsClip(GameObject player) {
        player.GetComponent<NetworkPlayerController>().AudioSourceFootsteps.Play();
    }

    /// <summary>
    /// Tells the server to pause the footsteps audio clip for a player.</summary>
    /// <param name="player">The player on which to player footsteps SFX.</param>
    [Command]
    public void CmdPauseFootstepsClip(GameObject player) {
        RpcPauseFootstepsClip(player);
    }

    /// <summary>
    /// Tells all clients to pause the footsteps audio clip for a player.</summary>
    /// <param name="player">The player on which to player footsteps SFX.</param>
    [ClientRpc]
    public void RpcPauseFootstepsClip(GameObject player) {
        player.GetComponent<NetworkPlayerController>().AudioSourceFootsteps.Pause();
    }

    /// <summary>
    /// Tells the server to apply a knockback force on the player.</summary>
    /// <param name="player">The player to knock backwards.</param>
    /// <param name="force">The force with which to knock them backwards.</param>
    [Command]
    private void CmdApplyKnockback(GameObject player, Vector3 force) {
        RpcApplyKnockback(player, force);
    }

    /// <summary>
    /// Tells all clients to apply a knockback force on the player.</summary>
    /// <param name="player">The player to knock backwards.</param>
    /// <param name="force">The force with which to knock them backwards.</param>
    [ClientRpc]
    private void RpcApplyKnockback(GameObject player, Vector3 force) {
        player.GetComponent<NetworkPlayerController>().PopVulnerableTimer = 1.5F;
        player.GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Moves the player's bubbleman.</summary>
    private void Move() {
        // no moving while punching
        if (!IsPunching) {
            // get user input
            float hAxis = Input.GetAxis(InputAxes.Move);

            if (hAxis == 0) {
                anim.SetInteger("Condition", 0);

                // stop footstep sound
                if (AudioSourceFootsteps.isPlaying) {
                    CmdPauseFootstepsClip(gameObject);
                }

                return;
            } else {
                if (!IsAttacking) {
                    anim.SetInteger("Condition", 1);

                    // play footstep sound
                    if (!AudioSourceFootsteps.isPlaying && !IsPlayerAirborne()) {
                        CmdPlayFootstepsClip(gameObject);
                    }
                }
            }

            // move the player in the input direction
            Vector3 movement = Vector3.right * hAxis * Speed * Time.deltaTime;
            rb.MovePosition(rb.position + movement);

            // ensure the player is facing the direction which they are moving
            if (movement != Vector3.zero) {
                rb.MoveRotation(Quaternion.LookRotation(movement));
            }
        }

        // stop footstep sound
        else if (AudioSourceFootsteps.isPlaying) {
            CmdPauseFootstepsClip(gameObject);
        }
    }

    /// <summary>
    /// Gets the player's input.</summary>
    private void GetInput() {
        if (Input.GetButtonDown(InputAxes.Jump) && !IsAttacking && !IsPlayerAirborne()) {
            // jump if player presses jump and is not already airborne
            Jump();

            // play jump noise which is the same as attack
            CmdPlaySFXClip(gameObject, 1, 1F);
        } else if (Input.GetButtonDown(InputAxes.Punch) && !IsAttacking) {
            // or, attack if the player isn't already attacking
            Attack();
        }
    }

    /// <summary>
    /// Returns whether the player is in the air.</summary>
    /// <returns>Returns true is the player is off the ground, false otherwise.</returns>
    private bool IsPlayerAirborne() {
        return !Physics.Raycast(transform.position + transform.up * (playerHeight / 2), -transform.up, (playerHeight / 2) + AirborneHeight, ~(1 << LayerMask.NameToLayer("Player")));
    }

    /// <summary>
    /// Player jump behaviour.</summary>
    private void Jump() {
        // apply an upwards force to player
        rb.velocity = Vector3.zero;
        rb.AddForce(Vector3.up * JumpStrength * Mathf.Sqrt(1 / rb.mass), ForceMode.VelocityChange);
    }

    /// <summary>
    /// Player attack behaviour.</summary>
    private void Attack() {
        if (IsPlayerAirborne()) {
            // set punching flag to false
            IsPunching = false;

            // enable kick collider
            Colliders.Kick.enabled = true;

            // play kick animation
            anim.SetTrigger("Kick");

            // play kick sound
            CmdPlaySFXClip(gameObject, 2, 1F);
        } else {
            // set punching flag to true
            IsPunching = true;

            // enable punch colliders
            Colliders.Punch.enabled = true;

            // play punch animation
            anim.SetTrigger("Attack");

            // play punch sound
            CmdPlaySFXClip(gameObject, 1, 1F);
        }
    }
}
