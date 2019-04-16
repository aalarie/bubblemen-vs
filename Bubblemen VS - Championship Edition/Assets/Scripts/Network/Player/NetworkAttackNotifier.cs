using UnityEngine;

/// <summary>
/// This class notifies the player controller when the player has started/ended their attack animation.</summary>
/// <remarks>This class is used in online mode only.</remarks>
public class NetworkAttackNotifier : StateMachineBehaviour {

    /// <summary>
    /// This function is called on the first frame of the state being played.</summary>
    /// <param name="animator">Thenimator that this state machine behaviour is on.</param>
    /// <param name="stateInfo">The current info for the state that the state machine behaviour is on.</param>
    /// <param name="layerIndex">The layer the state machine behaviour's state is on.</param>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.GetComponent<NetworkPlayerController>().IsAttacking = true;
    }

    /// <summary>
    /// This function is called on the last frame of a transition to another state.</summary>
    /// <param name="animator">The animator that this state machine behaviour is on.</param>
    /// <param name="stateInfo">The current info for the state that the state machine behaviour is on.</param>
    /// <param name="layerIndex">The layer the state machine behaviour's state is on.</param>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.GetComponent<NetworkPlayerController>().IsAttacking = false;

        // disable attack collider
        animator.GetComponent<NetworkPlayerController>().ResetAttackColliderEnabled();
    }
}
