using UnityEngine;
using UnityEngine.AI;
public class DoorController : MonoBehaviour
{
    [SerializeField] private float openHeight = 3f;
    private bool isOpen = false;
    private NavMeshObstacle navMeshObstacle;
    private void Awake()
    {
        navMeshObstacle = GetComponent<NavMeshObstacle>();
    }

    [ContextMenu("Test Open Door")]
    public void OpenDoor()
    {
        if (isOpen)
            return;
        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = false;
        }
        transform.position += Vector3.up * openHeight;
        isOpen = true;
        Debug.Log("Kapı Açıldı.");
    }
}
