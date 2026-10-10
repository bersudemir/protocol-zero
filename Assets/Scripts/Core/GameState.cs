using UnityEngine;

public class GameState : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private bool hasKeycard;
    [SerializeField] private bool powerRestored;

    public bool HasKeycard => hasKeycard;
    public bool PowerRestored => powerRestored;

    public void SetHasKeycard(bool value)
    {
        hasKeycard = value;
    }

    public void SetPowerRestored(bool value)
    {
        powerRestored = value;
    }
}