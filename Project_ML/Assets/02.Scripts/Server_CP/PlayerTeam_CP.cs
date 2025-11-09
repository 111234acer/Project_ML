using UnityEngine;
using Photon.Pun;

/// <summary>
/// 클라이언트 권위형 팀 관리 스크립트
/// 0: Red, 1: Blue
/// </summary>
public class PlayerTeam_CP : MonoBehaviourPun
{
    [Tooltip("0: Red, 1: Blue")]
    public int teamNumber = -1;

    void Awake()
    {
        if (!photonView.IsMine) return;

        // 로컬에서 팀 미지정이면 자동 배정
        if (teamNumber == -1)
        {
            teamNumber = AutoAssignTeam();
            photonView.RPC(nameof(RPC_SetTeam), RpcTarget.AllBuffered, teamNumber);
        }
    }

    int AutoAssignTeam()
    {
        int redCount = 0, blueCount = 0;

        foreach (var player in FindObjectsOfType<PlayerTeam_CP>())
        {
            if (player.teamNumber == 0) redCount++;
            else if (player.teamNumber == 1) blueCount++;
        }

        // 더 적은 팀에 배정
        return (redCount <= blueCount) ? 0 : 1;
    }

    [PunRPC]
    void RPC_SetTeam(int t)
    {
        teamNumber = Mathf.Clamp(t, 0, 1);

        // 팀별 시각 효과 / 재질 / 태그 적용 가능
        // ex) if (teamNumber == 0) bodyMaterial.color = Color.red;
        //     else bodyMaterial.color = Color.blue;
    }
}
