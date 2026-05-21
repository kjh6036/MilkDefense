using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoUI : MonoBehaviour
{
    [Header("공통")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Image _gradeBadgeImage;
    [SerializeField] private TextMeshProUGUI _gradeBadgeText;

    [Header("스탯 (라벨 / 값 쌍)")]
    [SerializeField] private TextMeshProUGUI _statLabelLeft;
    [SerializeField] private TextMeshProUGUI _statValueLeft;
    [SerializeField] private TextMeshProUGUI _statLabelRight;
    [SerializeField] private TextMeshProUGUI _statValueRight;
    [SerializeField] private TextMeshProUGUI _statLabelLeft2;
    [SerializeField] private TextMeshProUGUI _statValueLeft2;
    [SerializeField] private TextMeshProUGUI _statLabelRight2;
    [SerializeField] private TextMeshProUGUI _statValueRight2;

    [Header("스킬 버튼")]
    [SerializeField] private List<Button> _skillButtons;
    [SerializeField] private TextMeshProUGUI _skillDescText;

    [Header("등급별 배지 색상")]
    [SerializeField] private Color _colorCommon = new Color(0.06f, 0.20f, 0.38f);
    [SerializeField] private Color _colorUncommon = new Color(0.06f, 0.30f, 0.15f);
    [SerializeField] private Color _colorRare = new Color(0.25f, 0.10f, 0.45f);
    [SerializeField] private Color _colorEpic = new Color(0.50f, 0.35f, 0.00f);
    [SerializeField] private Color _colorBoss = new Color(0.23f, 0.00f, 0.06f);

    private string _currentDescription = string.Empty;

    private void Awake()
    {
        _panel.SetActive(false);
        SetSkillButtonsActive(0);

        if (_skillButtons != null)
            for (int i = 0; i < _skillButtons.Count; i++)
            {
                int index = i;
                _skillButtons[i].onClick.AddListener(() => OnSkillButtonClicked(index));
            }
    }

    // ─── 용병 ─────────────────────────────────────────────

    private SkillData[] _currentSkills;

    public void ShowMercenary(EntityInfoData data, MercenaryStatData statData, SkillData[] skills = null)
    {
        SetHeader(data, statData.grade.ToString(), GradeColor(statData.grade));
        SetStats(
            "공격력", statData.attackDamage.ToString("F0"),
            "사거리", statData.attackRange.ToString("F1"),
            "공격속도", $"{1f / statData.attackCooldown:F1}/s",
            "등급", statData.grade.ToString()
        );
        _currentSkills = skills;
        SetSkillButtonsActive(skills != null ? skills.Length : 0);
        if (_skillDescText != null) _skillDescText.text = string.Empty;
        _panel.SetActive(true);
    }

    // ─── 닭 ───────────────────────────────────────────────

    public void ShowChicken(EntityInfoData data, Grade grade, int level)
    {
        SetHeader(data, grade.ToString(), GradeColor(grade));
        SetStats(
            "등급", grade.ToString(),
            "레벨", $"Lv {level}",
            string.Empty, string.Empty,
            string.Empty, string.Empty
        );
        SetSkillButtonsActive(0);
        _panel.SetActive(true);
    }

    // ─── 적 ───────────────────────────────────────────────

    public void ShowEnemy(EntityInfoData data, EnemyStatData statData)
    {
        string badgeLabel = statData.enemyType == EnemyType.Boss ? "Boss" : statData.enemyType.ToString();
        Color badgeColor = statData.enemyType == EnemyType.Boss ? _colorBoss : _colorCommon;

        SetHeader(data, badgeLabel, badgeColor);
        SetStats(
            "HP", statData.maxHp.ToString("F0"),
            "이동속도", statData.moveSpeed.ToString("F1"),
            string.Empty, string.Empty,
            string.Empty, string.Empty
        );
        SetSkillButtonsActive(0);
        _panel.SetActive(true);
    }

    // ─── 기본 ─────────────────────────────────────────────

    public void Show(EntityInfoData data)
    {
        SetHeader(data, string.Empty, Color.clear);
        ClearStats();
        SetSkillButtonsActive(0);
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    // ─── 내부 ─────────────────────────────────────────────

    private void SetHeader(EntityInfoData data, string badgeLabel, Color badgeColor)
    {
        _iconImage.sprite = data.icon;
        _nameText.text = data.entityName;
        _currentDescription = data.description;
        _descriptionText.text = data.description;

        bool hasBadge = !string.IsNullOrEmpty(badgeLabel);
        if (_gradeBadgeImage != null) _gradeBadgeImage.gameObject.SetActive(hasBadge);
        if (hasBadge)
        {
            if (_gradeBadgeImage != null) _gradeBadgeImage.color = badgeColor;
            if (_gradeBadgeText != null) _gradeBadgeText.text = badgeLabel;
        }
    }

    private void SetStats(
        string label1, string value1,
        string label2, string value2,
        string label3, string value3,
        string label4, string value4)
    {
        SetStat(_statLabelLeft, _statValueLeft, label1, value1);
        SetStat(_statLabelRight, _statValueRight, label2, value2);
        SetStat(_statLabelLeft2, _statValueLeft2, label3, value3);
        SetStat(_statLabelRight2, _statValueRight2, label4, value4);
    }

    private void SetStat(TextMeshProUGUI label, TextMeshProUGUI value, string labelStr, string valueStr)
    {
        bool active = !string.IsNullOrEmpty(labelStr);
        if (label != null) { label.gameObject.SetActive(active); label.text = labelStr; }
        if (value != null) { value.gameObject.SetActive(active); value.text = valueStr; }
    }

    private void ClearStats()
    {
        SetStats(
            string.Empty, string.Empty,
            string.Empty, string.Empty,
            string.Empty, string.Empty,
            string.Empty, string.Empty
        );
    }

    private void OnSkillButtonClicked(int index)
    {
        if (_currentSkills == null || index >= _currentSkills.Length) return;

        SkillData skill = _currentSkills[index];
        _descriptionText.text = $"[{skill.skillName}]\n{skill.description}";
    }

    public void ResetDescription()
    {
        _descriptionText.text = _currentDescription;
    }

    private void SetSkillButtonsActive(int count)
    {
        if (_skillButtons == null) return;
        for (int i = 0; i < _skillButtons.Count; i++)
            _skillButtons[i].gameObject.SetActive(i < count);
    }

    private Color GradeColor(Grade grade) => grade switch
    {
        Grade.Common => _colorCommon,
        Grade.Uncommon => _colorUncommon,
        Grade.Rare => _colorRare,
        Grade.Epic => _colorEpic,
        _ => _colorCommon
    };
}