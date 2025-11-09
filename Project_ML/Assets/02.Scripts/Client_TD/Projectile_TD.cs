using UnityEngine;

public class Projectile_TD : MonoBehaviour
{
    public float damage; // 공격력

    [SerializeField] private LayerMask destroyOnLayers;
    [SerializeField] private bool destroyOnMonster = true;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    /// 발사 시 공격력 초기화
    public void Init(float dmg)
    {
        damage = dmg;
    }

    /// 적과 충돌 시 데미지 적용 후 삭제
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Monster_TD enemy))
        {
            enemy.TakeDamage(damage);

            if (destroyOnMonster)
            {
                Destroy(gameObject);
            }
            return;
        }

        int otherBit = 1 << other.gameObject.layer;
        if ((destroyOnLayers.value & otherBit) != 0)
        {
            Destroy(gameObject);
            return;
        }
    }
}
