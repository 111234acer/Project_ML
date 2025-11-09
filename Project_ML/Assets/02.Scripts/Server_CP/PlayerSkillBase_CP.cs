using UnityEngine;
using System.Collections;

public abstract class PlayerSkillBase_CP : MonoBehaviour
{
    public float cooldown = 5f;
    bool onCD = false;

    public bool CanUse() => !onCD;

    protected void StartCD()
    {
        if (onCD) return;
        onCD = true;
        StartCoroutine(CoolRoutine());
    }

    IEnumerator CoolRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        onCD = false;
    }

    public abstract void Use();
}
