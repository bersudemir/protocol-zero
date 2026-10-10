using UnityEngine;
[RequireComponent(typeof(CompanionNavigator))]
public class CompanionController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2f;
    private CompanionNavigator navigator;
    private void Awake()
    {
        navigator = GetComponent<CompanionNavigator>();
    }
    private void Update()
    {
        FollowPlayer();
    }
    private void FollowPlayer()
    {
        if (player == null)
            return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > followDistance)
        {
            navigator.MoveTo(player);
        }
        else
        {
            navigator.Stop();
        }
    }
}
