using UnityEngine;
[RequireComponent(typeof(Collider))]

public class CompanionSensor : MonoBehaviour
{
    [SerializeField] private float sensorDistance = 3f;
    [SerializeField] private float sideAngle = 45f;

    public bool ForwardBlocked { get; private set; }
    public bool LeftBlocked { get; private set; }
    public bool RightBlocked { get; private set; }

    private Collider companionCollider;

    private void Awake()
    {
        companionCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        SenseEnvironment();
    }

    private void SenseEnvironment()
    {
        Vector3 origin = companionCollider.bounds.center;
        Vector3 forwardDirection = transform.forward;
        Vector3 leftDirection = Quaternion.Euler(0f, sideAngle, 0f) * forwardDirection;
        Vector3 rightDirection = Quaternion.Euler(0f, -sideAngle, 0f) * forwardDirection;
        ForwardBlocked = CastSensor(origin, forwardDirection);
        LeftBlocked = CastSensor(origin, leftDirection);
        RightBlocked = CastSensor(origin, rightDirection);
    }

    private bool CastSensor(Vector3 origin, Vector3 direction)
    {
        bool hit = Physics.Raycast(origin, direction,out RaycastHit hitInfo, sensorDistance);
        Debug.DrawRay(origin, direction * sensorDistance, hit ? Color.red : Color.green);
        return hit;
    }
}
