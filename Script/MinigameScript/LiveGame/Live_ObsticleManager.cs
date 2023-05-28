using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Live_ObsticleManager : MonoBehaviour
{
    private const float DEFAULT_OBSTICLE_LIFE_TIME = 5.0f;

    [SerializeField] private LiveGameManager SCRIPT_LiveGameManager = null;
    [SerializeField] private GameObject PF_Obsticle = null;
    [SerializeField] private float f_ObsticleSpeed = 0;

    [System.Serializable] public class MovingObsticle
    {
        public float lifeTime = DEFAULT_OBSTICLE_LIFE_TIME;
        public SphereCollider collider;
        public Transform transform;
        public Transform child;
    }
    [SerializeField] private List<MovingObsticle> L_MovingObsticle;
    [SerializeField] private List<Transform> L_SpawnPoint = new List<Transform>(8);

    private int i_MaxObsticleNumber = 0;

    private Transform TRAN_Character;
    private BoxCollider COL_Character;


    public void Dispose()
    {
        for(int i = 0; i < i_MaxObsticleNumber; i++)
        {
            L_MovingObsticle[i].transform.gameObject.SetActive(false);
        }

        StopCoroutine("StartShooting");
        StopCoroutine("ObsticleUpdate");

        enabled = false;
    }

    public void Initialize(int _ObsticleNumver, Transform _Character)
    {
        TRAN_Character = _Character;
        COL_Character = _Character.GetComponent<BoxCollider>();
        i_MaxObsticleNumber = _ObsticleNumver;

        L_MovingObsticle = new List<MovingObsticle>(i_MaxObsticleNumber);

        if(i_MaxObsticleNumber > 0)
        {
            for (int i = 0; i < i_MaxObsticleNumber; i++)
            {
                GameObject obsticle = Instantiate(PF_Obsticle, Vector3.zero, Quaternion.identity);
                L_MovingObsticle.Add(new MovingObsticle());
                L_MovingObsticle[i].collider = obsticle.GetComponent<SphereCollider>();
                L_MovingObsticle[i].transform = obsticle.transform;
                L_MovingObsticle[i].child = obsticle.transform.GetChild(0).transform;
                obsticle.SetActive(false);
            }

            StartCoroutine("StartShooting");
            StartCoroutine("ObsticleUpdate");
        }
    }

    private IEnumerator ObsticleUpdate()
    {
        while (true)
        {
            for (int i = 0; i < i_MaxObsticleNumber; i++)
            {
                if (L_MovingObsticle[i].transform.gameObject.active)
                {
                    L_MovingObsticle[i].transform.localPosition += L_MovingObsticle[i].transform.forward * Time.deltaTime * f_ObsticleSpeed;
                    L_MovingObsticle[i].child.Rotate(Vector3.right * Time.deltaTime * 300.0f);

                    // 5초간 이동후 초기화
                    if ((L_MovingObsticle[i].lifeTime -= Time.deltaTime) < 0.0f)
                    {
                        L_MovingObsticle[i].transform.gameObject.SetActive(false);
                        L_MovingObsticle[i].lifeTime = DEFAULT_OBSTICLE_LIFE_TIME;
                        continue;
                    }

                    // 캐릭터와 충돌시 초기화 및 캐릭터 프리징
                    if (L_MovingObsticle[i].collider.bounds.Intersects(COL_Character.bounds))
                    {
                        SCRIPT_LiveGameManager.FreezCharacter();
                        L_MovingObsticle[i].lifeTime = DEFAULT_OBSTICLE_LIFE_TIME;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator StartShooting()
    {
        int current = 0;

        while (true)
        {
            // 발사 딜레이 초기화
            float obsticleDelay = Random.Range(3f, 10f);

            // 위치 초기화
            int position = Random.Range(0, 7);
            L_MovingObsticle[current].transform.position = L_SpawnPoint[position].position;

            // 캐릭터를 바라보게 설정
            L_MovingObsticle[current].transform.LookAt(TRAN_Character);
            Vector3 rot = L_MovingObsticle[current].transform.localEulerAngles;
            rot.x = rot.z = 0;
            L_MovingObsticle[current].transform.localEulerAngles = rot;

            // 장애물 활성화
            L_MovingObsticle[current].transform.gameObject.SetActive(true);

            // 발사 딜레이
            yield return new WaitForSeconds(obsticleDelay);
            
            // 다음 발사 초기화
            if (++current >= L_MovingObsticle.Count)
                current = 0;

            while (L_MovingObsticle[current].transform.gameObject.active)
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
