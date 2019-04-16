using UnityEngine;

/// <summary>
/// Script attached to an attack collider to detect collisions with other players.</summary>
/// <remarks>This class is used in online mode only.</remarks>
public class NetworkAttackCollider : MonoBehaviour {

    /// <summary>
    /// Whether this attack is a punch or a kick.</summary>
    public bool IsPunch;

    /// <summary>
    /// This function is called when the <c>Collider</c> other enters the trigger.</summary>
    /// <param name="other">The other <c>Collider</c> involved in this collision.</param>
    private void OnTriggerEnter(Collider other) {
        // check if attack hits a player
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            GetComponentInParent<NetworkPlayerController>().OnAttackCollided(other.gameObject, IsPunch);
        }
    }
}
