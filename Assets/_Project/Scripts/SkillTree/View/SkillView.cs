using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SkillView : MonoBehaviour
{
    [SerializeField] private Skill _skill;
    [SerializeField] private TMP_Text _upgradeProgressText;
    [SerializeField] private SkillDescription _skillDescription;
    [SerializeField] private Image _baseOutline;
    [SerializeField] private Image _maxUpgradeOutline;
    [SerializeField] private Color _cannotAffordUpgradeColor = Color.red;

    private void Awake()
    {
        _skill.OnLock += HandleSkillLock;
        _skill.OnUnlock += HandleSkillUnlock;
        _skill.OnUnlock += UpdatePlayersAbilityToBuyUpgrade;
        _skill.OnUpgrade += HandleSkillUpgrade;
        _skill.OnHover += HandleSkillHover;
        _skill.OnHoverExit += HandleSkillHoverExit;

        _skill.EconomyService.Coins
            .Subscribe(_ => UpdatePlayersAbilityToBuyUpgrade())
            .AddTo(this);
        _skill.EconomyService.OnPetalChanged
            .Subscribe(_ => UpdatePlayersAbilityToBuyUpgrade())
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _skill.OnLock -= HandleSkillLock;
        _skill.OnUnlock -= HandleSkillUnlock;
        _skill.OnUnlock -= UpdatePlayersAbilityToBuyUpgrade;
        _skill.OnUpgrade -= HandleSkillUpgrade;
        _skill.OnHover -= HandleSkillHover;
        _skill.OnHoverExit -= HandleSkillHoverExit;
    }

    private void HandleSkillLock()
    {
        gameObject.SetActive(false);
    }

    private void HandleSkillUnlock()
    {
        gameObject.SetActive(true);
    }

    private void HandleSkillHover()
    {
        _skillDescription.Show(_skill.Name, _skill.Description);
    }

    private void HandleSkillHoverExit()
    {
        _skillDescription.Hide();
    }

    private void HandleSkillUpgrade(int level)
    {
        _upgradeProgressText.text = $"{level}/{_skill.MaxLevel}";

        if (level >= _skill.MaxLevel)
        {
            _baseOutline?.gameObject?.SetActive(false);
            _maxUpgradeOutline?.gameObject?.SetActive(true);
        }
        else
        {
            _baseOutline?.gameObject?.SetActive(true);
            _maxUpgradeOutline?.gameObject?.SetActive(false);
        }
    }

    private void UpdatePlayersAbilityToBuyUpgrade()
    {
        if (_skill.CanUpgrade() || _skill.CurrentLevel >= _skill.MaxLevel)
        {
            _baseOutline.color = Color.white;
        }
        else
        {
            _baseOutline.color = _cannotAffordUpgradeColor;
        }
    }

    private void OnDrawGizmos()
    {
        if (_skill == null)
            return;

        // Draw arrows to all child objects
        Gizmos.color = Color.black;
        Vector3 currentPosition = transform.position;

        for (int i = 0; i < _skill.Children.Count; i++)
        {
            Skill skill = _skill.Children[i];

            if (skill == null)
                continue;

            Transform child = skill.transform;
            Vector3 childPosition = child.position;

            // Draw line from current position to child
            Gizmos.DrawLine(currentPosition, childPosition);

            // Draw arrow head
            Vector3 direction = (childPosition - currentPosition).normalized;
            Vector3 right = Vector3.Cross(Vector3.forward, direction).normalized;
            Vector3 arrowHead1 = childPosition - direction * 20f + right * 10f;
            Vector3 arrowHead2 = childPosition - direction * 20f - right * 10f;

            Gizmos.DrawLine(childPosition, arrowHead1);
            Gizmos.DrawLine(childPosition, arrowHead2);
        }
    }
}
