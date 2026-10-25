
using UnityEngine;

public class GameState : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private bool hasKeycard;
    [SerializeField] private bool powerRestored;

    public bool HasKeycard => hasKeycard;
    public bool PowerRestored => powerRestored;

    // Oyun durumu her deðiþtiðinde artýrýlýr.
    public int StateVersion { get; private set; }

    public void SetHasKeycard(bool value)
    {
        if (hasKeycard == value)
            return;

        hasKeycard = value;
        StateVersion++;
    }

    public void SetPowerRestored(bool value)
    {
        if (powerRestored == value)
            return;

        powerRestored = value;
        StateVersion++;
    }
}
