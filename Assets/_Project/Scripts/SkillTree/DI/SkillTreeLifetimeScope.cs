using System.Collections.Generic;
using UnityEngine;
using VContainer;

public static class SkillTreeLifetimeScopeExtension
{
    public static void RegisterSkillTreeSystems(this IContainerBuilder builder, List<GameObject> autoInjectGameObjects, Transform skillsParent)
    {
        autoInjectGameObjects.AddRange(GetAllChildGameObjects(skillsParent));
    }

    private static GameObject[] GetAllChildGameObjects(Transform parent)
    {
        var childObjects = new List<GameObject>();

        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);

            childObjects.AddRange(GetAllChildGameObjects(child));

            if (child.TryGetComponent<Skill>(out _) == false)
            {
                continue;
            }

            childObjects.Add(child.gameObject);
        }

        return childObjects.ToArray();
    }
}
