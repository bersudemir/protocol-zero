using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVisualFacing : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 540f;
    [SerializeField] private float modelYawOffSet = 0f;
    private PlayerController playerController;
    private Quaternion lastTargetRotation;
    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        lastTargetRotation = transform.rotation;
    }
    private void LateUpdate()
    {
        if (playerController == null)
            return;
        Vector2 input = playerController.MoveInput;
        Vector3 direction = new Vector3(input.x, 0f, input.y);
        if(Keyboard.current!=null&&
            Keyboard.current.cKey.isPressed&&
            Camera.main != null)
        {
            direction = Camera.main.transform.position - transform.position;
        }

        direction.y = 0f;
        if (direction.sqrMagnitude >= 0.0001f)
        {
            lastTargetRotation= Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(0f, modelYawOffSet, 0f);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lastTargetRotation,turnSpeed * Time.deltaTime);
    }
}
