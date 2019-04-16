using UnityEngine;

/// <summary>
/// Class for handling player behaviour in the fighting stage.</summary>
public class PlayerController : MonoBehaviour {

    // Audio Source
    private AudioSource audioSourceSFX;
    private AudioSource audioSourceFootsteps;
    public AudioClip footstepsClip;
    public AudioClip punchingClip;
    public AudioClip kickingClip;
    public AudioClip poppingClip;
    public AudioClip bouncingClip;

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
    private GameManager gm;

    /// <summary>
    /// The attack collider which is currently enabled.</summary>
    private Collider activeAttackCollider;

    /// <summary>
    /// This function is called on the frame when a script is enabled just before any of the Update methods is called the first time.</summary>
    private void Start() {
        gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
        RespawnPoint = transform.position + 3F * transform.up;

        // get components on player
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // find the height and radius of the player
        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        playerRadius = GetComponent<CapsuleCollider>().radius * transform.localScale.x;

        // determine the pop threshold for the player based on their size
        PopThreshold = 3F * Mathf.Sin(transform.localScale.y) - 4F * Mathf.Cos(transform.localScale.y) + 7F;

        // Manage audiosources
        AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
        audioSourceSFX = audioSources[0];
        audioSourceFootsteps = audioSources[1];
        audioSourceFootsteps.clip = footstepsClip;
        audioSourceFootsteps.loop = true;
    }

    /// <summary>
    /// This function is called every frame, if the <c>MonoBehaviour</c> is enabled.</summary>
    private void Update() {
        // decrement pop vulnerability timer
        PopVulnerableTimer -= Time.deltaTime;
        if (PopVulnerableTimer < 0F) {
            PopVulnerableTimer = 0F;
        }

        // prevent queued kicking
        if (IsPlayerAirborne()) {
            anim.ResetTrigger("Kick");
        }

        GetInput();
        Move();

        // If airborne stop footstep noises that are playing
        if (audioSourceFootsteps.isPlaying && IsPlayerAirborne())
            audioSourceFootsteps.Pause();
    }

    /// <summary>
    /// This function is called when this collider/rigidbody has begun touching another rigidbody/collider.</summary>
    /// <param name="collision">The Collision data associated with this collision.</param>
    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Player") {
            // If we collide with other player, play bump sound
            audioSourceSFX.PlayOneShot(bouncingClip, 0.25f);
        }

        // pop the player if they hit a surface too hard
        if (PopVulnerableTimer > 0F && Vector3.Magnitude(collision.impulse) >= PopThreshold) {
            PopPlayer();
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
    public void PopPlayer() {
        // Play pop sound
        audioSourceSFX.PlayOneShot(poppingClip, 1.0f);
        if (name == "Player 1") {
            // increment player 2's score
            gm.PlayerTwoScore++;

            // respawn player is game continues
            if (gm.PlayerTwoScore < gm.WinScore) {
                PopVulnerableTimer = 0F;
                transform.position = RespawnPoint;
                rb.velocity = Vector3.zero;
            } else {
                Destroy(gameObject);
            }
        } else {
            // increment player 1's score
            gm.PlayerOneScore++;

            // respawn player is game continues
            if (gm.PlayerOneScore < gm.WinScore) {
                PopVulnerableTimer = 0F;
                transform.position = RespawnPoint;
                rb.velocity = Vector3.zero;
            } else {
                Destroy(gameObject);
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
    /// Moves the player's bubbleman.</summary>
    private void Move() {
        // no moving while punching
        if (!IsPunching) {
            // get user input
            float hAxis = Input.GetAxis(InputAxes.Move);

            if (hAxis == 0) {
                anim.SetInteger("Condition", 0);

                // Stop footstep sound
                if (audioSourceFootsteps.isPlaying)
                    audioSourceFootsteps.Pause();
                return;
            } else {
                if (!IsAttacking) {
                    anim.SetInteger("Condition", 1);

                    // play footstep sound
                    if (!audioSourceFootsteps.isPlaying && !IsPlayerAirborne())
                        audioSourceFootsteps.Play();
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

        // Stop footstep sound
        else if (audioSourceFootsteps.isPlaying)
            audioSourceFootsteps.Pause();
    }

    /// <summary>
    /// Gets the player's input.</summary>
    private void GetInput() {
        if (Input.GetButtonDown(InputAxes.Jump) && !IsAttacking && !IsPlayerAirborne()) {
            // jump if player presses jump and is not already airborne
            Jump();
            // play jump noise which is the same as attack
            audioSourceSFX.PlayOneShot(punchingClip, 1.0f);
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
            audioSourceSFX.PlayOneShot(kickingClip, 1.0f);
        } else {
            // set punching flag to true
            IsPunching = true;

            // enable punch colliders
            Colliders.Punch.enabled = true;

            // play punch animation
            anim.SetTrigger("Attack");

            // play punch sound
            audioSourceSFX.PlayOneShot(punchingClip, 1.0f);
        }
    }
}
