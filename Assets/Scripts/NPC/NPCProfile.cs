using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( //profil asset oluþturmak için bir menü öðesi ekler
    fileName = "NewNPCProfile",
    menuName = "Protocol Zero/NPC Profile"
)]
public class NPCProfile : ScriptableObject
{
    [SerializeField] private string npcName;

    [TextArea(2, 4)]
    [SerializeField] private string personality;

    [TextArea(2, 4)]
    [SerializeField] private string objective;

    [SerializeField] private string[] allowedActions;

    //kapsülleme-encapsulation -> public propertyler ile private fieldlere eriþim saðlanýr
    public string NpcName => npcName;
    public string Personality => personality;
    public string Objective => objective;
    public IReadOnlyList<string> AllowedActions => allowedActions;
}