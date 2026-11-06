
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class FinalExitTrigger : MonoBehaviour
{
    [Header("Exit References")]
    [SerializeField] private MuratExitDoorBridge doorBridge;

    private bool endingStarted = false;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Bitis zaten basladiysa tekrar calisma.
        if (endingStarted)
            return;

        // Kapi acilmamissa oyun bitmesin.
        if (doorBridge == null || !doorBridge.ExitOpened)
            return;

        // Sadece oyuncu cikis bolgesini tetikleyebilir.
        bool isPlayer =
            other.CompareTag("Player") ||
            other.transform.root.CompareTag("Player");

        if (!isPlayer)
            return;

        endingStarted = true;

        Debug.Log("Oyuncu cikisa ulasti. Oyun tamamlandi.");

        SceneManager.LoadScene("EndingScene");
    }
}
