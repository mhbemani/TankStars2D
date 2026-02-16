using UnityEngine;
public class DestroyOnExit : StateMachineBehaviour {
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Destroy(animator.gameObject); // Deletes the explosion when the animation ends
    }
}