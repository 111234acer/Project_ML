using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class PlayerHealth_Server : MonoBehaviourPunCallbacks
{
    [Header("Components")]
    [SerializeField] CharacterController characterController;   // CC만 사용
    [SerializeField] Collider[] deathToggleColliders;           // 비우면 자동 수집
    Dictionary<Collider, bool> _colBackup;
    PhotonView pv;
    GameManager gm;

    [Header("Character Stats")]
    public int maxHealth = 200;
    public int attackDamage = 20;
    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool isDead;

    [Header("Respawn Settings")]
    public float respawnDelay = 8f;

    [Header("Invincible Settings")]
    public float invincibleTime = 0.2f;
    public float respawnInvincibleTime = 2f;
    bool isInvincible;

    float invincibleUntil;

    private AnimationHandler animationHandler;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
        gm = FindObjectOfType<GameManager>();
        if (deathToggleColliders == null || deathToggleColliders.Length == 0)
            deathToggleColliders = GetComponentsInChildren<Collider>(true);
        currentHealth = maxHealth;

        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (PhotonNetwork.IsMasterClient)
            pv?.RPC(nameof(Client_SetHealth), RpcTarget.Others, currentHealth);
    }

    [PunRPC]
    public void Client_RegisterDamageDealtTo(int targetViewId, float showSeconds)
    {
        OtherPlayerHealthBar.RegisterDamagedByLocal(targetViewId, showSeconds);
    }

    // 서버 권위 데미지 처리 (PhotonMessageInfo 포함 호출용)
    [PunRPC]
    public void Server_ApplyDamage(int dmg, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient || isDead || isInvincible) return;

        int newHp = Mathf.Max(0, currentHealth - Mathf.Max(0, dmg));
        if (newHp == currentHealth) return;  // 변화 없으면 종료
        currentHealth = newHp;
        pv.RPC(nameof(Client_SetHealth), RpcTarget.Others, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Invincible(invincibleTime));
            photonView.RPC(nameof(Client_Anim_Hit), RpcTarget.All);
        }
    }

    // [추가] AOE / 화살 등에서 PhotonMessageInfo 없이 호출할 때 호환용
    [PunRPC]
    public void Server_ApplyDamage(int dmg)
    {
        if (!PhotonNetwork.IsMasterClient || isDead || isInvincible)
            return;

        int newHp = Mathf.Max(0, currentHealth - Mathf.Max(0, dmg));
        if (newHp == currentHealth) return;  // 변화 없으면 종료
        currentHealth = newHp;
        pv.RPC(nameof(Client_SetHealth), RpcTarget.Others, currentHealth);

        photonView.RPC(nameof(Client_Anim_Hit), RpcTarget.All);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Invincible(invincibleTime));
        }
    }

    public void RequestHeal(int amount)
    {
        amount = Mathf.Max(0, amount);
        if (PhotonNetwork.IsMasterClient)
        {
            Server_Heal(amount, default);
        }
        else
            pv.RPC(nameof(Server_Heal), RpcTarget.MasterClient, amount);
    }

    [PunRPC]
    void Server_Heal(int amount, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient || isDead) return;

        int newHp = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
        if (newHp == currentHealth) return;
        currentHealth = newHp;
        pv.RPC(nameof(Client_SetHealth), RpcTarget.Others, currentHealth);
    }

    [PunRPC]
    void Client_SetHealth(int hp)
    {
        currentHealth = Mathf.Clamp(hp, 0, maxHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        photonView.RPC("Client_Anim_Dead", RpcTarget.All);

        SetPassThrough(true);
        if (characterController) characterController.enabled = false;

        gm?.RequestRespawn(pv, respawnDelay);
    }

    // GameManager에서 호출하는 표준 리스폰 엔드포인트(전 클라 동기화)
    [PunRPC]
    public void RPC_RespawnAt(Vector3 pos, Vector3 forward, int hp, float invuln = 1.0f)
    {
        transform.SetPositionAndRotation(pos, Quaternion.LookRotation(forward, Vector3.up));

        currentHealth = Mathf.Clamp(hp, 1, maxHealth);
        isDead = false;

        SetPassThrough(false);
        if (characterController) characterController.enabled = true;

        StartCoroutine(Invincible(Mathf.Max(invuln, respawnInvincibleTime)));

        if (currentHealth > 0)
            photonView.RPC("Client_Anim_Respawn", RpcTarget.All);
    }

    IEnumerator Invincible(float dur)
    {
        isInvincible = true;
        invincibleUntil = Mathf.Max(invincibleUntil, Time.time + dur);
        while (Time.time < invincibleUntil) yield return null;
        isInvincible = false;
    }

    void SetPassThrough(bool enable)
    {
        var cols = (deathToggleColliders != null && deathToggleColliders.Length > 0)
            ? deathToggleColliders
            : GetComponentsInChildren<Collider>(true);

        if (enable)
        {
            if (_colBackup == null) _colBackup = new Dictionary<Collider, bool>();
            _colBackup.Clear();

            foreach (var c in cols)
            {
                if (!c || c is CharacterController) continue;
                _colBackup[c] = c.isTrigger;
                c.isTrigger = true; // 모두 통과
            }
        }
        else
        {
            if (_colBackup != null)
            {
                foreach (var kv in _colBackup)
                    if (kv.Key) kv.Key.isTrigger = kv.Value;
                _colBackup.Clear();
            }
        }
    }

    // 보조 API(호환)
    public float GetHealthPercent() => (float)currentHealth / Mathf.Max(1, maxHealth);
    public int GetAttackDamage() => attackDamage;
    public bool IsDead() => isDead;

    [PunRPC]
    void Client_Anim_Dead()
    {
        animationHandler?.OnDead();
    }

    [PunRPC]
    void Client_Anim_Respawn()
    {
        animationHandler?.Respawn();
    }

    [PunRPC]
    void Client_Anim_Hit()
    {
        animationHandler?.HitTrigger();
    }
}
