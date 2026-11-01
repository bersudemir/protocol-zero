
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private PlayerController playerController;
    private bool? previousMoving;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (animator == null || playerController == null)
            return;

        bool isMoving =
            playerController.MoveInput.sqrMagnitude > 0.01f;

        // Durum deðiþmediyse animasyonu yeniden baþlatma.
        if (previousMoving.HasValue &&
            previousMoving.Value == isMoving)
            return;

        previousMoving = isMoving;

        animator.CrossFade(
            isMoving ? "Walk" : "Idle1",
            0.15f,
            0
        );
    }
}
