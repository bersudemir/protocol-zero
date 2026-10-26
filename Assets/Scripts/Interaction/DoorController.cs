using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class DoorController : MonoBehaviour
{

    [Header("Kapı Ayarları")]
    [SerializeField] private float slideDistance = 3.5f;
    [SerializeField] private float slideDuration = 1.5f;
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
        isOpen = true;
        StartCoroutine(SlideDoor());
    }
    private IEnumerator SlideDoor()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.right * slideDistance;
        float elapsedTime = 0f;
        Debug.Log("FirstGate yana kayılarak açılıyor.");
        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / slideDuration);
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            yield return null;
        }
        transform.position = endPosition;
        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = false;
        }
        Debug.Log("Kapı tamamen açıldı.");
    }
}
