using UnityEngine;

/// <summary>
/// This class notifies the player controller when the player has started/ended their attack animation.</summary>
public class AttackNotifier : StateMachineBehaviour {

    /// <summary>
    /// This function is called on the first frame of the state being played.</summary>
    /// <param name="animator">Thenimator that this state machine behaviour is on.</param>
    /// <param name="stateInfo">The current info for the state that the state machine behaviour is on.</param>
    /// <param name="layerIndex">The layer the state machine behaviour's state is on.</param>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.GetComponent<PlayerController>().IsAttacking = true;
    }

    /// <summary>
    /// This function is called on the last frame of a transition to another state.</summary>
    /// <param name="animator">The animator that this state machine behaviour is on.</param>
    /// <param name="stateInfo">The current info for the state that the state machine behaviour is on.</param>
    /// <param name="layerIndex">The layer the state machine behaviour's state is on.</param>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.GetComponent<PlayerController>().IsAttacking = false;

        // disable attack collider
        animator.GetComponent<PlayerController>().ResetAttackColliderEnabled();
    }
}
