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
        /*
        if (collision.collider.TryGetComponent(out EnemyHealth_TD enemy))       // Enemy 체력 관련 스크립트 추가해야함
        {
            enemy.TakeDamage(damage);
        }
        */

        Destroy(gameObject);
    }
}
