using UnityEngine;

public class SkillView : MonoBehaviour
{
    [SerializeField] private Skill _skill;

    private void Awake()
    {
        _skill.OnLock += HandleSkillLock;
        _skill.OnUnlock += HandleSkillUnlock;
        _skill.OnUpgrade += HandleSkillUpgrade;
    }

    private void OnDestroy()
    {
        _skill.OnLock -= HandleSkillLock;
        _skill.OnUnlock -= HandleSkillUnlock;
        _skill.OnUpgrade -= HandleSkillUpgrade;
    }

    private void HandleSkillLock()
    {
        gameObject.SetActive(false);
    }

    private void HandleSkillUnlock()
    {
        gameObject.SetActive(true);
    }

    private void HandleSkillUpgrade(int level)
    {
        // Update the UI to reflect the new skill level
    }

    private void OnDrawGizmos()
    {
        if (_skill == null)
            return;

        // Draw arrows to all child objects
        Gizmos.color = Color.yellow;
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
