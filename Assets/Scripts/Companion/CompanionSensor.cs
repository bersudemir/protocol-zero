using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Collider))]

public class CompanionSensor : MonoBehaviour
{
    [SerializeField] private float sensorDistance = 3f;
    [SerializeField] private float sideAngle = 45f;

    public bool ForwardBlocked { get; private set; }
    public bool LeftBlocked { get; private set; }
    public bool RightBlocked { get; private set; }
    public Vector3 AgentPosition { get; private set; }
    public float ForwardDistance { get; private set; }
    public float LeftDistance { get; private set; }
    public float RightDistance { get; private set; }
    public float ForwardAngle { get; private set; }
    public float LeftAngle { get; private set; }
    public float RightAngle { get; private set; }

    private Collider companionCollider;
    [SerializeField] private bool showSensorLogs = true;
    private float nextSensorLogTime = 0f;

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
        AgentPosition = transform.position;
        Vector3 origin = companionCollider.bounds.center;
        Vector3 forwardDirection = transform.forward;
        Vector3 leftDirection = Quaternion.Euler(0f, -sideAngle, 0f) * forwardDirection;
        Vector3 rightDirection = Quaternion.Euler(0f, sideAngle, 0f) * forwardDirection;
        ForwardBlocked = CastSensor(
            origin,
            forwardDirection,
            out float forwardDistance,
            out float forwardAngle);

        LeftBlocked = CastSensor(
            origin,
            leftDirection,
            out float leftDistance,
            out float leftAngle);
        RightBlocked = CastSensor(
            origin, 
            rightDirection,
            out float rightDistance,
            out float rightAngle);
        ForwardDistance = forwardDistance;
        LeftDistance = leftDistance;
        RightDistance = rightDistance;
        ForwardAngle = forwardAngle;
        LeftAngle = leftAngle;
        RightAngle = rightAngle;

        if(showSensorLogs && Time.time >= nextSensorLogTime)
        {
            nextSensorLogTime = Time.time + 2f;
            string frontInfo = ForwardBlocked
                ? $"{ForwardDistance:F2} m / {ForwardAngle:F0} derece"
                : "Engel yok";
            string leftInfo = LeftBlocked
                ? $"{LeftDistance:F2} m / {LeftAngle:F0} derece"
                : "Engel yok";
            string rightInfo = RightBlocked
                ? $"{ForwardDistance:F2} m / {RightAngle:F0} derece"
                : "Engel yok";
            Debug.Log(
                $"[COMPANION SENSOR] " +
                $"Konum: {AgentPosition.ToString("F2")} | " +
                $"Ön: {frontInfo} | " +
                $"Sol: {leftInfo} | " +
                $"Sağ: {rightInfo}");
        }

    }

    private bool CastSensor(Vector3 origin, Vector3 direction, out float obstacleDistance, out float obstacleAngle)
    {
        bool hit = Physics.Raycast(origin, direction,out RaycastHit hitInfo, sensorDistance,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
        Debug.DrawRay(origin, direction * sensorDistance, hit ? Color.red : Color.green);
        if (hit)
        {
            obstacleDistance = hitInfo.distance;
            obstacleAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        }
        else
        {
            obstacleDistance = float.PositiveInfinity;
            obstacleAngle = float.NaN;
        }
        Debug.DrawRay(origin, direction * (hit ? hitInfo.distance : sensorDistance),
            hit ? Color.red : Color.green);
        return hit;
    }
}
