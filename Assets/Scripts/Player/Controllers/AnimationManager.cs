using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;

    private readonly int isMovingHash = Animator.StringToHash("isMoving");
    private readonly int shootHash = Animator.StringToHash("Shoot");

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        ConversionPower.OnShootFired += HandleShoot;
    }

    private void OnDisable()
    {
        ConversionPower.OnShootFired -= HandleShoot;
    }

    private void Update()
    {
        if (playerController == null) return;
        HandleMovementAnimation();
    }

    private void HandleMovementAnimation()
    {
        bool isMoving = playerController.MoveInput.sqrMagnitude > 0.01f;
        animator.SetBool(isMovingHash, isMoving);
    }

    private void HandleShoot(PlayerController player)
    {
        if (player != playerController) return;
        animator.SetTrigger(shootHash);
    }
}