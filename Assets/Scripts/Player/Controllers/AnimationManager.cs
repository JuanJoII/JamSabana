using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    
    private readonly int isMovingHash = Animator.StringToHash("isMoving");
    private readonly int putObjectHash = Animator.StringToHash("putObject");

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
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
    
    public void TriggerPutObject()
    {
        animator.SetTrigger(putObjectHash);
    }
}