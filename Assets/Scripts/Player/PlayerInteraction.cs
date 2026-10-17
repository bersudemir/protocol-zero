using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private TextMeshProUGUI interactionPrompt;
    private void Update()
    {
        if (Keyboard.current == null)
            return;
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionRadius);
        foreach (Collider nearbyCollider in nearbyColliders)
        {
            MonoBehaviour[] behaviours =
                nearbyCollider.GetComponentsInParent<MonoBehaviour>();
            foreach(MonoBehaviour behaviour in behaviours)
            {
                if(behaviour is IInteractable interactable)
                { 
                    interactable.Interact();
                    return;
                }
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
