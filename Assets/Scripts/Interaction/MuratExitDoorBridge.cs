
using UnityEngine;

public class MuratExitDoorBridge : MonoBehaviour
{
    [Header("Murat ve Kapi Baglantilari")]
    [SerializeField] private DialogueUIController dialogueUI;
    [SerializeField] private DoorController finalExitDoor;

    public bool TryOpenExit()
    {
        if (dialogueUI == null || finalExitDoor == null)
        {
            Debug.LogWarning(
                "MuratExitDoorBridge: Gerekli referanslar eksik."
            );
            return false;
        }

        if (!dialogueUI.TryConsumeExitPermission())
        {
            Debug.Log(
                "MuratExitDoorBridge: Gecerli cikis izni yok."
            );
            return false;
        }

        finalExitDoor.OpenDoor();

        Debug.Log(
            "MuratExitDoorBridge: Cikis izni kullanildi."
        );

        return true;
    }
}
