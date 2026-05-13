using UnityEngine;

[CreateAssetMenu(fileName = "EntityInfoData", menuName = "Game/Entity/InfoData")]
public class EntityInfoData : ScriptableObject
{
    [Header("Info")]
    public string entityName;
    [TextArea] public string description;
    public Sprite icon;
}