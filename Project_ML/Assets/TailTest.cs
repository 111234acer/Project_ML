using System.Collections;
using UnityEngine;

public class TailTest : MonoBehaviour
{
    // 1. 이동 속도 (N 변수)를 Inspector에서 설정할 수 있게 public float으로 선언
    public float speed = 5f; // 초당 5 유닛 (예시 값)

    void Start()
    {
        // 2. 게임 시작 시 'MoveAfterDelay' 코루틴을 시작합니다.
        StartCoroutine(MoveAfterDelay());
    }

    // Coroutine은 IEnumerator 타입을 반환하며, 'yield return'을 사용하여 실행을 잠시 멈출 수 있습니다.
    IEnumerator MoveAfterDelay()
    {
        // 3. 여기서 1초 동안 기다립니다.
        yield return new WaitForSeconds(1f); 

        // 4. 1초 대기가 끝나면, 오브젝트를 계속 이동시키기 위한 무한 루프를 시작합니다.
        while (true)
        {
            // Z축 (Vector3.forward) 방향으로 'speed'만큼, Time.deltaTime을 곱해 프레임 속도에 관계없이 초당 일정한 속도로 이동시킵니다.
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);

            // 5. 다음 프레임까지 기다립니다. 이 부분이 없으면 게임이 멈춥니다.
            yield return null; 
        }
    }

    // Update 함수는 이제 사용하지 않습니다.
    // void Update()
    // {
    //     
    // }
}