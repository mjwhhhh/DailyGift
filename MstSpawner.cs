using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MstSpawner : SpawnZoneBase
{
    public List<OnGameEvent> GameEvents = new List<OnGameEvent>();

    public SpawnPointEnemy m_Spawner;

    void Start()
    {
        foreach (OnGameEvent gameEvent in GameEvents)
            GameBlackboard.Instance.GameEvents.AddEventChangeHandler(gameEvent.Name, EventHandler);


        CheckMstInfo();
    }
    
    public void EventHandler(string name, GameEvents.E_State state)
    {
        foreach (OnGameEvent gameEvent in GameEvents)
        {
            if (GameBlackboard.Instance.GameEvents.GetState(gameEvent.Name) != gameEvent.State)
                return;
        }
        //Debug.LogError("Handle Event Spawn");
        SendEventsWhenDone = true;
        //StartSpawn();
        StartCoroutine(SpawnNextRound());
    }
    [SerializeField]
    public RoundInfo[] m_RoundInfo;

    public void SetRoundInfo(List<RoundInfo> RInfo)
    {
        //m_RoundInfo.Clear();
        m_RoundInfo = null;
        m_RoundInfo = new RoundInfo[RInfo.Count];

        RInfo.CopyTo(m_RoundInfo);


        Debug.Log("m_RoundInfo Count:" + m_RoundInfo.Length);
        CheckMstInfo();
    }

    void CheckMstInfo()
    {
        Debug.Log("Round Count:" + m_RoundInfo.Length);

        for (int i = 0; i < m_RoundInfo.Length; i++)
        {
            Debug.Log("R" + (i + 1).ToString() + " Wave Count:" + m_RoundInfo[i].WavesInfos.Length);
        }
        Debug.Log("Mst Count:" + m_RoundInfo.Length);
    }

    public int NowRound = -1;
    IEnumerator SpawnNextRound()
    {
        NowRound++;

        //mjw 设置显示器轮数内容 0 = 1
        csGameManager.m_Instance.ShowLevel(NowRound + 1, 0);

        State = E_State.E_SPAWNING_ENEMIES;
        yield return new WaitForSeconds(m_RoundInfo[NowRound].RoundTime);
        Debug.Log(m_RoundInfo[NowRound].RoundName + " Started");

        float formalWaveTime = 0;
        for(int i=0; i < m_RoundInfo[NowRound].RoundWaveNum; i ++)
        {
            
            WaveInfo c_WaveInfo = m_RoundInfo[NowRound].WavesInfos[i];
#if UNITY_EDITOR
            if (c_WaveInfo.WaveTime - formalWaveTime < 0)
            {
                Debug.LogError("current wave start time is smaller than former one.");
                yield break;
            }
#endif
            yield return new WaitForSeconds(c_WaveInfo.WaveTime - formalWaveTime);
            formalWaveTime = c_WaveInfo.WaveTime;

            float formalMstTime = 0;
            for(int j =0; j < c_WaveInfo.WaveMstNum; j ++)
            {
                yield return new WaitForSeconds(c_WaveInfo.MstInfos[j].SpawnTime - formalMstTime);
                formalMstTime = c_WaveInfo.MstInfos[j].SpawnTime;
                MstSpawnInfo c_mstInfo = c_WaveInfo.MstInfos[j];
                m_Spawner.EnemyType = c_mstInfo.EnemyType;
                m_Spawner.RoomToInit = c_mstInfo.RoomToInit;
                m_Spawner.IsTranspos = c_mstInfo.GoTransport;
                m_Spawner.posNum = c_mstInfo.InitPos;
                m_Spawner.InitState = SpawnPointEnemy.E_InitState.Patrol;

                StartCoroutine(SpawnEnemy(m_Spawner));
                yield return new WaitForSeconds(0.01f);
            }
        }
        State = E_State.E_IN_PROGRESS;

        while (GetEnemyCount()>0)
        {
            yield return new WaitForSeconds(5);
        }

        // 当前轮打完

        if(NowRound + 1 < m_RoundInfo.Length)
        {
            StartCoroutine(SpawnNextRound());
        }
        else
        {
            //mjw 关卡结束，结算界面
            csGameManager.m_Instance.ShowLevel(NowRound + 1, 2);
        }

        yield return new WaitForSeconds(0.01f);
    }

    public void StopSpawn()
    {
        //跳出spawn协程

       foreach( Agent x in  EnemiesAlive)
        {
            x.DeadByGameZone();
        }
    }
}
