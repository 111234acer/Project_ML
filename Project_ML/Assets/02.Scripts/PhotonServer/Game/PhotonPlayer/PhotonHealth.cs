using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class PhotonHealth : MonoBehaviour
{
    [Header("Character Info")]
    public string characterName = "";

    [Header("Character Stats")]
    public int MaxHealth = 100;               // 최대 체력
    public int attackDamage;            // 공격력
    public int localHp;                 // 로컬에서 계산할 hp (계산 후 다른 클라이언트로 중계)
    public int currentHealth;           // 현재 체력 (중계받은 Hp) (실제 체력게이지에 적용될 수치)
    public int prevHealth;              // 캐릭터 사망시 안정적으로 중계해주기 위한 변수
    public bool isDead = false;         // 사망 여부

    public int localKill = 0;
    public int localDeath = 0;
    public int killCount = 0;
    public int deathCount = 0;

    [Header("Respawn Settings")]
    public float respawnDelay = 8f;     // 리스폰 시간
    public Transform spawnPoint;        // 지정 스폰 포인트

    int     playerId = -1;
    string  playerTeam = "blue";
    PhotonView pv;

    ExitGames.Client.Photon.Hashtable props_CurHp = new ExitGames.Client.Photon.Hashtable(); // 플레이어의 현재체력 중계변수
    ExitGames.Client.Photon.Hashtable props_KD = new ExitGames.Client.Photon.Hashtable();    // 플레이어의 킬데스 중계변수
    private MeshRenderer[] mrs;

    const string curHpKey = "CurHp";
    const string lastAttackerIdKey = "LastAttackerId";
    const string killKey = "Kill";
    const string deathKey = "Death";

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        mrs = GetComponentsInChildren<MeshRenderer>();
    }

    private void Start()
    {
        playerId = pv.Owner.ActorNumber;
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            playerTeam = (string)pv.Owner.CustomProperties["MyTeam"];
        
        localHp         = MaxHealth;
        prevHealth      = MaxHealth;
        currentHealth   = MaxHealth;      // 게임 시작 시 체력을 최대 체력으로 초기화

        InitCustomProperties();

        Debug.Log($"{characterName} 시작 ! 채력 : {currentHealth}, 공격력 : {attackDamage}");          // 로그로 캐릭터 시작 상태 출력
    }

    private void Update()
    {
        ReceiveCurHp();
        ReceiveKillCount();
        ReceiveDeathCount();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name.Contains("Arrow")) // 화살판별 임시코드
        {
            PhotonArrow arrow = other.GetComponent<PhotonArrow>();
            arrow.GetComponent<Collider>().enabled = false;
            if (playerId == arrow.ownerId)
                return;
            Debug.LogWarning("화살 맞음!");
            TakeDamage(10, arrow.ownerId, arrow.teamOfOwner);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage">                               </param>
    /// <param name="attakerId">    공격자의 ActorNumber    </param>
    /// <param name="atkTeam">      공격자의 팀(blue, red)   </param>
    public void TakeDamage(int damage, int attakerId, string atkTeam) // 데미지 함수
    {
        if (!pv.IsMine)             return;
        Debug.LogWarning("1");
        //if (playerTeam == atkTeam)  return;                           // 같은 팀의 공격이라면 무시 (팀킬이 가능하게 하려면 블록해 주세요!)
        if (isDead)                 return;                           // 죽으면 데미지 무시
        Debug.LogWarning("2");
        if (currentHealth <= 0)
            return;
        Debug.LogWarning("3");


        localHp -= damage;                                      // 체력을 데미지 받는 만큼 깍고
        localHp = Mathf.Max(localHp, 0);                        // 최소값을 0으로 제한


        Debug.Log($"{characterName} 피해 {damage} -> 남은 체력 : {currentHealth}");

        SendCurHp(attakerId);
        // 사망 처리는 체력 중계( ReceiveCurHp() )했을 때 처리 (모든 클라이언트에서 동시에 호출시키기 위해)
        //if (localHp <= 0) // 체력이 0이되면 Die 함수 실행
        //{
        //Die();
        //}
    }

    public void Heal(int amount) // 체력을 회복하는 함수
    {
        if (isDead) return;          // 죽으면 힐 불가능
        currentHealth += amount;    // amount 만큼 체력을 회복
        currentHealth = Mathf.Min(currentHealth, MaxHealth); // 최대 체력을 넘지않도록 제한

        Debug.Log($"{characterName} 회복 {amount} -> 체력 : {currentHealth}");  // 로그로 회복 -> 체력 상태 출력
    }

    void Die()  // 사망 처리 함수
    {
        if (isDead) return;      // 이미 죽어있으면 중복 실행 방지
        isDead = true;

        Debug.LogWarning($"{characterName} 사망!");
        IncreaseDeathCount();
        // 사망 에니메이션 추가 예정
        SetVisible(false);
        StartCoroutine(Respawn());      // 리스폰 되는 코루틴 실행
    }

    void SetVisible(bool isVisible) // 플레이어가 죽었을 때 모습을 감추거나 리스폰 됐을 때 모습을 보이게 합니다.
    {
        foreach (MeshRenderer mr in mrs)
        {
            mr.enabled = isVisible;
        }

        Rigidbody[] rigidbodys = this.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rigidbody in rigidbodys)
        {
            rigidbody.isKinematic = !isVisible;
        }
        BoxCollider[] colls = this.GetComponentsInChildren<BoxCollider>(true);
        foreach (BoxCollider coll in colls)
        {
            coll.enabled = isVisible;
        }
    }
    // 리스폰 처리
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay); // 리스폰 시간 기다린 후 리스폰

        // 위치 초기화
        if (spawnPoint != null)              // spawnPoint가 지정 되어있으면 해당 위치로 이동
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        // 체력 리셋
        currentHealth = MaxHealth;
        isDead = false;

        Debug.Log($"{characterName} 리스폰 완료 → 체력 {currentHealth}");
    }

    // 현재 체력 비율 (UI용)
    public float GetHealthPercent()
    {
        return (float)currentHealth / MaxHealth;
    }

    // 공격력 가져오기 (공격할 때 사용)
    public int GetAttackDamage()
    {
        return attackDamage;
    }

    public bool IsDead()
    {
        return isDead;
    }

    #region 플레이어 체력,킬,데스 중계 관련 함수
    void InitCustomProperties()
    {
        if (pv != null && pv.IsMine == true)
        {
            // 플레이어 체력 중계변수
            props_CurHp.Clear();
            props_CurHp.Add(curHpKey, localHp);
            props_CurHp.Add(lastAttackerIdKey, -1);      // 나를 막타친 플레이어의 ActorNumber
            pv.Owner.SetCustomProperties(props_CurHp);

            // 플레이어 킬뎃 중계변수
            props_KD.Clear();
            props_KD.Add(killKey, 0);
            props_KD.Add(deathKey, 0);
            pv.Owner.SetCustomProperties(props_KD);
        }
    }

    void SendCurHp(int lastAttackerId) // 자신의 체력을 다른 플레이어에게 중계
    {
        if (pv == null) return;
        if (!pv.IsMine) return;
        if(props_CurHp == null) // 만에 하나라도 프로퍼티가 초기화 되어있지 않았을 때
        {
            props_CurHp = new ExitGames.Client.Photon.Hashtable();
            props_CurHp.Clear();
        }

        if (props_CurHp.ContainsKey(curHpKey) == true)
            props_CurHp[curHpKey] = localHp;
        else
            props_CurHp.Add(curHpKey, localHp);

        if (props_CurHp.ContainsKey(lastAttackerIdKey) == true)
            props_CurHp[lastAttackerIdKey] = lastAttackerId;
        else
            props_CurHp.Add(lastAttackerIdKey, lastAttackerId);

        pv.Owner.SetCustomProperties(props_CurHp);
    }   

    void ReceiveCurHp() // 다른 클라이언트에서 중계해 준 HP 정보 받아오기 (내가 중계한 내 HP도 여기서 받음)
    {
        if (pv == null) return;
        if (pv.Owner == null) return; // 플레이어가 나갔을 경우 예외

        if(pv.Owner.CustomProperties.ContainsKey(curHpKey) == true)
        {
            currentHealth = (int)pv.Owner.CustomProperties[curHpKey];

            // 체력게이지 기능 작성해주세요.


            // 플레이어 사망 처리
            if(prevHealth > 0 && currentHealth <= 0)
            {
                if(pv.Owner.CustomProperties.ContainsKey(lastAttackerIdKey) == true)
                {
                    int killerId = (int)pv.Owner.CustomProperties[lastAttackerIdKey];
                    if(killerId >= 0)
                    {
                        // 해당 유저의 킬카운트 증가 시키기
                        FindKiller(killerId);
                    }
                }

                // 플레이어 사망처리
                Die();
            }

            prevHealth = currentHealth;
        }
    }

    void FindKiller(int killerId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            // ************ 수정할 때 컴포넌트명 확인해 주세요! ******************
            var playerHealth = player.GetComponent<PhotonHealth>();
            if(playerHealth != null && playerHealth.playerId == killerId)
            {
                playerHealth.IncreaseKillCount();
                return;
            }
        }
    }

    void IncreaseKillCount()
    {
        if(pv != null && pv.IsMine == true)
        {
            if(pv.Owner.CustomProperties.ContainsKey(killKey) == true)
            {
                localKill++;
                SendKillCount(localKill);
            }
        }
    }

    void IncreaseDeathCount()
    {
        if (pv != null && pv.IsMine == true)
        {
            if (pv.Owner.CustomProperties.ContainsKey(deathKey) == true)
            {
                localDeath++;
                SendDeathCount(localDeath);
            }
        }
    }

    void SendKillCount(int killCount)
    {
        if (pv == null) return;

        if (pv.IsMine == false) return;

        if(props_KD == null)
        {
            props_KD = new ExitGames.Client.Photon.Hashtable();
            props_KD.Clear();
        }

        if (props_KD.ContainsKey(killKey))
            props_KD[killKey] = killCount;
        else
            props_KD.Add(killKey, killCount);

        pv.Owner.SetCustomProperties(props_KD);
    }

    void SendDeathCount(int deathCount)
    {
        if (pv == null) return;

        if (pv.IsMine == false) return;

        if (props_KD == null)
        {
            props_KD = new ExitGames.Client.Photon.Hashtable();
            props_KD.Clear();
        }

        if (props_KD.ContainsKey(deathKey))
            props_KD[deathKey] = deathCount;
        else
            props_KD.Add(deathKey, deathCount);

        pv.Owner.SetCustomProperties(props_KD);
    }

    void ReceiveKillCount()
    {
        if (pv == null)
            return;
        if (pv.Owner == null)
            return;

        if (pv.Owner.CustomProperties.ContainsKey(killKey) == true)
        {
            int a_KillCount = (int)pv.Owner.CustomProperties[killKey];
            if (killCount != a_KillCount)
            {
                killCount = a_KillCount;
            }
        }
    }

    void ReceiveDeathCount()
    {
        if (pv == null)
            return;
        if (pv.Owner == null)
            return;

        if (pv.Owner.CustomProperties.ContainsKey(deathKey) == true)
        {
            int a_DeathCount = (int)pv.Owner.CustomProperties[deathKey];
            if (deathCount != a_DeathCount)
            {
                deathCount = a_DeathCount;
            }
        }
    }
    #endregion
}