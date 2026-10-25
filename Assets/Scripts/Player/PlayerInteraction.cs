using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private TextMeshProUGUI interactionPrompt;
    private IInteractable currentInteractable;
    private void Awake()
    {
        if(interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        currentInteractable = FindClosestInteractable();
        UpdateInteractionPrompt();

        if(Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if(currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
    }

    private IInteractable FindClosestInteractable()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionRadius);
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach(Collider nearbyCollider in nearbyColliders)
        {
            MonoBehaviour[] behaviours = nearbyCollider.GetComponentsInParent<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if(behaviour is IInteractable interactable && behaviour.isActiveAndEnabled)
                {
                    float distance = Vector3.Distance(transform.position, nearbyCollider.ClosestPoint(transform.position));
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                    break;
                }
            }
        }
        return closestInteractable;
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt == null)
            return;
        bool shouldShow = currentInteractable != null;

        if (shouldShow)
        {
            if(currentInteractable is ControlPanel)
            {
                interactionPrompt.text = "[E] Companion'a kapıyı açtır.";
            }
            else if (currentInteractable is KeycardRequestPanel)
            {
                interactionPrompt.text = "[E] Companion'dan erişim kartını getirmesini iste.";
            }
            else
            {
                interactionPrompt.text = "[E] Etkileşim kur.";
            }
        }
        if(interactionPrompt.gameObject.activeSelf != shouldShow)
        {
            interactionPrompt.gameObject.SetActive(shouldShow);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
