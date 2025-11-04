using UnityEngine;

public class Projectile_TD : MonoBehaviour
{
    public float damage; // 공격력

    /// 발사 시 공격력 초기화
    public void Init(float dmg)
    {
        damage = dmg;
    }

    /// 적과 충돌 시 데미지 적용 후 삭제
    private void OnCollisionEnter(Collision collision)
    {        
        if (collision.collider.TryGetComponent(out Monster_TD enemy))
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
