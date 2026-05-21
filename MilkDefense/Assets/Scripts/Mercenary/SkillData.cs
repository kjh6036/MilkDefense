using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Game/Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
}
