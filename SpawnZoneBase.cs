using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class SpawnZoneBase : MonoBehaviour
{
    public enum E_ShowFirstTimeEnemy
    {
        No,
        Yes,
    };

    [System.Serializable]
    public class ShowFirstEnemy
    {
        public float Delay = 0;
        public GameObject Camera;
        public AnimationClip CameraAnim;
        public float FadeOutTime = 0.3f;
        public float FadeInTime = 0.3f;
        public Transform PlayerFinalPos;
        public GameObject Enemy;
    }

    public E_ShowFirstTimeEnemy ShowFirstTimeEnemy = E_ShowFirstTimeEnemy.No;
    public ShowFirstEnemy FirstEnemyShow;
    public List<SpawnPointEnemy> SpawnPoints = null;
    public int SendEventWhenNumberOfEnemiesLeft = 0;
    public List<GameEvent> GameEventsToSend = new List<GameEvent>();
    public List<GameEvent> GameEventsToSendWhenDone = new List<GameEvent>();

    private GameObject GameObject;
	public List<Agent> EnemiesAlive = new List<Agent>();
    private bool SendEvents;
    public bool SendEventsWhenDone;

    private GameZone MyGameZone;

	public bool IsActive() { return EnemiesAlive.Count > 0; }
    public Agent GetEnemy(int index) { return EnemiesAlive[index]; }
    public int GetEnemyCount() { return EnemiesAlive.Count; }

    public enum E_State
    {
        E_WAITING_FOR_START,
        E_SPAWNING_ENEMIES,
        E_IN_PROGRESS,
        E_FINISHED,
    }

    
    public E_State State {get; set;}


	void Awake()
	{
        State = E_State.E_WAITING_FOR_START;
        SendEvents = true;
        SendEventsWhenDone = true;

        GameObject = gameObject;
        MyGameZone = GameObject.transform.root.GetComponent<GameZone>();
	}

    public void Enable()
    {
        //Debug.Log(name + " enable " + Mission.Instance.CurrentGameZone.name);

        if (ShowFirstTimeEnemy == E_ShowFirstTimeEnemy.Yes)
        {
            if(FirstEnemyShow.Camera)
                FirstEnemyShow.Camera.SetActiveRecursively(false);

            if (FirstEnemyShow.Enemy)
                FirstEnemyShow.Enemy.SetActiveRecursively(false);
        }

        GameObject.SetActiveRecursively(true);
    }

    public void Disable()
    {
        //Debug.Log(name + " disable " + Mission.Instance.CurrentGameZone.name);

        StopAllCoroutines();

        GameObject.SetActiveRecursively(false);
    }

    
    // We'll draw a gizmo in the scene view, so it can be found....
    void OnDrawGizmos()
    {
        BoxCollider b = GetComponent("BoxCollider") as BoxCollider;
        if (b != null)
        {
            Gizmos.color  = new Color(0,1,1,1); // Color.aqua
            Gizmos.matrix = b.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(b.center, b.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        else
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(transform.position, Vector3.one);
        }

        if(SpawnPoints != null)
        {
            for (int i = 0; i < SpawnPoints.Count; i++)
            {
                if (b != null)
                    Gizmos.DrawLine(b.transform.position + b.center, SpawnPoints[i].transform.position);
                else
                    Gizmos.DrawLine(gameObject.transform.position , SpawnPoints[i].transform.position);
            }
        }
    }

    /*void OnEnable()
    {
        Debug.Log(GameObject.name + " OnEnable");
    }
    void OnDisable()
    {
        Debug.Log(GameObject.name + " Ondisable");
    }*/

	// Update is called once per frame
	void FixedUpdate()
	{
        if (State != E_State.E_IN_PROGRESS)
            return;

        for (int i = EnemiesAlive.Count - 1; i >= 0; i--)
        {
      
            if (EnemiesAlive[i].IsAlive == true)
                continue;

			//Debug.LogError("EnemiesAlive :: remove enemy :: " + EnemiesAlive[i].name);
			EnemiesAlive.RemoveAt(i);

            //if (SendEvents && SendEventWhenNumberOfEnemiesLeft == EnemiesAlive.Count)
            //{
            //    foreach (GameEvent gameEvent in GameEventsToSend)
            //        Mission.Instance.SendGameEvent(gameEvent.Name, gameEvent.State, gameEvent.Delay);

            //    SendEvents = false;
            //}

            //if (SendEventsWhenDone && EnemiesAlive.Count == 0)
            //{
            //    foreach (GameEvent gameEvent in GameEventsToSendWhenDone)
            //        Mission.Instance.SendGameEvent(gameEvent.Name, gameEvent.State, gameEvent.Delay);

            //    SendEventsWhenDone = false;
            //}
        }
	}

	public void Reset()
	{
       // Debug.Log(GameObject.name + " Restart");
        
		StopAllCoroutines();

        State = E_State.E_WAITING_FOR_START;
        SendEvents = true;
        SendEventsWhenDone = true;

        EnemiesAlive.Clear();
    }

    protected void StartSpawn()
    {
//        Debug.Log(MyGameZone.gameObject.name +  " " + GameObject.name + " starting spanw !!");
        MyGameZone.EnableAllActiveInteractionInFight(false);

        //if (ShowFirstTimeEnemy == E_ShowFirstTimeEnemy.Yes) //过过场动画敌人 不需要这个方法
        //    StartCoroutine(SpawnEnemiesEx());
        //else
            StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        State = E_State.E_SPAWNING_ENEMIES;

        yield return new WaitForEndOfFrame();
        //Debug.LogError("Handle Event Spawn");

        for (int i = 0; i < SpawnPoints.Count; i++)
        {
            if (SpawnPoints[i].Difficulty > MissionData.Instance.GameDifficulty)
                continue;

            StartCoroutine(SpawnEnemy(SpawnPoints[i]));

            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(4.0f);

        State = E_State.E_IN_PROGRESS;
    }

    public IEnumerator SpawnEnemy(SpawnPointEnemy spawnpoint)
    {
        if(spawnpoint.EnemyType == E_EnemyType.None)
            yield break;
        //CombatEffectsManager.Instance.PlaySpawnEffect(spawnpoint.Transform.position, spawnpoint.Transform.forward);
        Transform m_SpawnPoint;
        //if (spawnpoint.EnemyType != E_EnemyType.BigSpider)
        {
            m_SpawnPoint = SpawnPosManger.instance.FindNowPos(spawnpoint.IsTranspos, spawnpoint.EnemyType, spawnpoint.posNum, spawnpoint.RoomToInit);
        }
        //else
        //{
        //    if (csGameManager.m_Instance.GetNowStationNum() >= 0)
        //    {
        //        m_SpawnPoint = SpawnPosManger.instance.FindNowBossPos();
        //    }
        //    else
        //    {
        //        yield break;
        //    }
        //}
        spawnpoint.DoorPos = m_SpawnPoint;
        GameObject enemy = Mission.Instance.GetAgendFromCache(spawnpoint.EnemyType, m_SpawnPoint);
        
        if (enemy == null)
        {
            Debug.LogError(this.name + " No enemy in cache " + spawnpoint.EnemyType + " on " + spawnpoint.name);
            yield break;
        }
        yield return new WaitForEndOfFrame();
        ComponentWeaponsAI w = enemy.GetComponent<ComponentWeaponsAI>();
        if (w != null)
        {
            w.Initialize();
        }

        //SentinelWeapon Sw = enemy.GetComponent<SentinelWeapon>();

        enemy.SendMessage("Activate", spawnpoint);

        Agent agent = enemy.GetComponent<Agent>();

        MyGameZone.AddEnemy(agent);
        EnemiesAlive.Add(agent);
//	     Debug.Log("EnemiesAlive :: add enemy :: " + agent.name);
    }

    SpawnPointEnemy GetAvailableSpawnPoint(SpawnPointEnemy[] spawnPoints)
    {
        Vector3 pos = csGameManager.m_Instance.playerList[0].playerRef.PlayerGame.Owner.Position;

        float bestValue = 0;
        int bestSpawn = -1; 

        for(int i = 0; i < spawnPoints.Length;i++)
        {
            if (MyGameZone.IsEnemyInRange(spawnPoints[i].transform.position, 2))
            {
            //    Debug.Log(i + " Spawnpoint " + spawnPoints[i].name + " is near to enemy");
                continue;
            }

            float value = 0;
            float dist = Mathf.Min(14, (spawnPoints[i].Transform.position - pos).magnitude);
            value = Mathfx.Hermite(0, 7, dist/7);

           // Debug.Log(i + " Spawnpoint " + spawnPoints[i].name + " dist " + dist + " Value " + value);
            if (value <= bestValue)
                continue;

            bestValue = value;
            bestSpawn = i;
        }

        //Debug.Log("Best spaqwn point is " + bestSpawn);

        if( bestSpawn == -1)
            return spawnPoints[Random.Range(0, spawnPoints.Length)];

        return spawnPoints[bestSpawn];
    }

//    private IEnumerator SendGameEvent(string name, GameEvents.E_State state, float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        GameBlackboard.Instance.GameEvents.Update(name, state);
//    }

}

