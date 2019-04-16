using UnityEngine;

/// <summary>
/// Script attached to an attack collider to detect collisions with other players.</summary>
public class AttackCollider : MonoBehaviour {

    // Audio Source
    public AudioSource audioSource;
    public AudioClip bubble_bumpClip;

    /// <summary>
    /// Whether this attack is a punch or a kick.</summary>
    public bool IsPunch;

    /// <summary>
    /// This function is called when the <c>Collider</c> other enters the trigger.</summary>
    /// <param name="other">The other <c>Collider</c> involved in this collision.</param>
    private void OnTriggerEnter(Collider other) {
        // check if attack hits a player
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            // pop player if they are against a wall
            if (other.GetComponentInParent<PlayerController>().IsPlayerAgainstWall()) {
                other.GetComponentInParent<PlayerController>().PopPlayer();
                return;
            }

            // get the player's knockback strength
            float strength;
            if (IsPunch) {
                strength = GetComponentInParent<PlayerController>().PunchStrength;
            } else {
                strength = GetComponentInParent<PlayerController>().KickStrength;
            }

            // make the other player vulnerable to being popped
            other.GetComponentInParent<PlayerController>().PopVulnerableTimer = 1.5F;

            // calculate the strength of the attack
            float sourceMass = GetComponentInParent<Rigidbody>().mass;
            float targetMass = other.GetComponentInParent<Rigidbody>().mass;
            float hitForce = sourceMass * (1 / targetMass) * strength;  // insert voodoo match magic here
            Debug.LogFormat("{0} hits {1} with {2} force", transform.root.name, other.transform.root.name, hitForce);

            // knock the other player backwards
            other.GetComponentInParent<Rigidbody>().AddForce(transform.root.forward * hitForce, ForceMode.VelocityChange);

            // Play bump sound
            audioSource.PlayOneShot(bubble_bumpClip, 0.5f);

            other.GetComponentInParent<Animator>().Play("get_hit");
        }
    }
}
