using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrollFSMFactoryPattern : MonoBehaviour
{
    public StateMachine stateMachine;


    ///
    public GameObject enemy;
    public float maxSpeed = 2;//2m/s 

    public float FOVinDEG = 70;
    private float cosOfFOVover2InRAD;  //cut off value for visibility checks

    public Transform[] waypoints;
    public int nextWaypointIndex = 0;

    public float maxAngularSpeedInDegPerSec = 60; //deg/sec
    public float maxAngularSpeedInRadPerSec; //rad/sec
    private float maxAngularSpeedInRadPerFrame;
    private static string REALIGN_WAY_POINT = "RealignWayPoint";
    private static string CHASE_ENEMY = "ChaseEnemy";
    private static string FIGHT_ENEMY = "FightEnemy";
    private static string SEEW_WAY_POINT = "SeekWayPoint";
    private static string CELEBRATION = "Celebration";
    ///
    public float celebrationTimer = 0;
    public int secondsToCelebrate = 2;

    public GameObject fireworkPrefab;
    

    // Start is called before the first frame update
    void Start()
    {
        cosOfFOVover2InRAD = Mathf.Cos(FOVinDEG / 2f * Mathf.Deg2Rad);
        maxAngularSpeedInRadPerSec = maxAngularSpeedInDegPerSec * Mathf.Deg2Rad;

        stateMachine = new StateMachine();

        StateMachine.State realignWayPoint = stateMachine.CreateState(REALIGN_WAY_POINT);
        realignWayPoint.onEnter = delegate { Debug.Log("realignWayPoint.onEnter"); };
        realignWayPoint.onExit = delegate { Debug.Log("realignWayPoint.onExit"); };
        realignWayPoint.onStay = delegate
        {
            //Debug.Log("realignWayPoint.onStay");
            DoRealignWaypoint();
        };

        StateMachine.State seekWayPoint = stateMachine.CreateState(SEEW_WAY_POINT);
        seekWayPoint.onEnter = delegate { Debug.Log("seekWayPoint.onEnter"); };
        seekWayPoint.onExit = delegate { Debug.Log("seekWayPoint.onExit"); };
        seekWayPoint.onStay = delegate
        {
            Debug.Log("seekWayPoint.onStay");
            //DEFAULT ACTION
            //print("HandleSeekWaypoint");
            DoSeekWaypoint();

            //CHECK FOR  TRANSITIONS
            //T4 - Waypoint Reached?
            if (Utilities.WaypointReached(this.transform.position, waypoints[nextWaypointIndex].position))
            {
                nextWaypointIndex = (nextWaypointIndex + 1) % waypoints.Length;
                //stateMachine.ChangeState("RealignWaypoint");
                stateMachine.ChangeState(REALIGN_WAY_POINT);
            }
            //T2 - SeeEnemy?
            if (null != enemy && Utilities.SeeEnemy(this.transform.position, enemy.transform.position, this.transform.forward, cosOfFOVover2InRAD))
            {
                stateMachine.ChangeState(CHASE_ENEMY);
            }

        };

        StateMachine.State chaseEnemy = stateMachine.CreateState(CHASE_ENEMY);
        chaseEnemy.onEnter = delegate { Debug.Log("chaseEnemy.onEnter"); };
        chaseEnemy.onExit = delegate { Debug.Log("chaseEnemy.onExit"); };
        chaseEnemy.onStay = delegate
        {
            Debug.Log("chaseEnemy.onStay");
            HandleChaseEnemy();

        };

        StateMachine.State fightEnemy = stateMachine.CreateState(FIGHT_ENEMY);
        fightEnemy.onEnter = delegate
        {
            Debug.Log("fightEnemy.onEnter");
            HandleFightEnemy();
        };
        fightEnemy.onExit = delegate { Debug.Log("fightEnemy.onExit"); };
        fightEnemy.onStay = delegate { Debug.Log("fightEnemy.onStay"); };

        StateMachine.State celebrate = stateMachine.CreateState(CELEBRATION);
        celebrate.onEnter = delegate
        {
            Debug.Log("celebrate.onEnter");
            Celebration();
        };
        celebrate.onExit = delegate { Debug.Log("celebrate.onExit"); };
        celebrate.onStay = delegate { Debug.Log("celebrate.onStay"); };


    }

    private void Celebration()
    {
        celebrationTimer += Time.deltaTime;
        int seconds = (int)celebrationTimer % 60;
        if (seconds >= secondsToCelebrate)
        {
            stateMachine.ChangeState(REALIGN_WAY_POINT);
            //transform.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            Debug.Log("1..........................................................");
            GameObject fw = Instantiate(fireworkPrefab);
            fw.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            Destroy(fw, 1);
        }
    }

    private void HandleFightEnemy()
    {
        //DEFAULT ACTION
        print("HandleFightEnemy");
        DoFightEnemy();

        //TRANSITION CHECKS
        //T5 - Enemy Dead or Lost Sight
        //if (EnemyDeadOrLostSight())
        if (Utilities.EnemyDeadOrLostSight(enemy.GetComponent<Health>(), enemy,this.gameObject,cosOfFOVover2InRAD))
        {
            //stateMachine.ChangeState(REALIGN_WAY_POINT);
            stateMachine.ChangeState(CELEBRATION);

        }
        //T6 - dit>2
        if (!Utilities.CheckDistanceLess(this.transform.position,enemy.transform.position,Utilities.DISTANCE_TO_FIGHT))
        {
            //ChangeState(TrollState.ChaseEnemy);
            stateMachine.ChangeState(CHASE_ENEMY);
        }

    }

    private void DoFightEnemy()
    {
        //throw new NotImplementedException();
        int damage = UnityEngine.Random.Range(0, 5);
        enemy.GetComponent<Health>().TakeDamage(damage);
    }

    private void DoRealignWaypoint()
    {
        //DEFAULT
        print("DoRealignWaypoint!!!!!!!");
        DoRealign();

        //TRANSITIONS
        //T1 - Aligned?
        //if (Utilities.IsAligned(this.transform.position, waypoints[nextWaypointIndex].position, this.transform.forward, 0.01f))
        if (IsAligned())
        {
            print("IsAligned...........................");
            stateMachine.ChangeState("SeekWayPoint");
        }
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }



    private void DoRealign()
    {


        //int i1 = (nextWaypointIndex + 1) % waypoints.Length;
        Vector3 headingToNextWP = waypoints[nextWaypointIndex].position - this.transform.position;
        headingToNextWP.Normalize();
        maxAngularSpeedInRadPerFrame = maxAngularSpeedInRadPerSec * Time.deltaTime;
        //Vector3 fwdWorld = this.transform.TransformVector(this.transform.forward);
        Vector3 newRotation = Vector3.RotateTowards(this.transform.forward, headingToNextWP, maxAngularSpeedInRadPerFrame, 0);
        this.transform.rotation = Quaternion.LookRotation(newRotation);
    }


    private void DoSeekWaypoint()
    {

        this.transform.position = Vector3.MoveTowards(this.transform.position, waypoints[nextWaypointIndex].position, maxSpeed * Time.deltaTime);
    }
    private bool IsAligned()
    {

        //int i1 = (nextWaypointIndex + 1) % waypoints.Length;
        Vector3 headingToNextWP = waypoints[nextWaypointIndex].position - this.transform.position;
        headingToNextWP.Normalize();
        float diff = Vector3.Distance(headingToNextWP, this.transform.forward);
        if (diff < 0.01)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool SeeEnemy()
    {
        if (null == enemy) return false;
        Vector3 T2Eheading = enemy.transform.position - this.transform.position;
        T2Eheading.Normalize();
        float cosTheta = Vector3.Dot(this.transform.forward, T2Eheading);
        return (cosTheta > cosOfFOVover2InRAD);
    }

    private void HandleChaseEnemy()
    {
        //DEFAULT
        print("HandleChaseEnemy");
        DoChaseEnemy();

        //Check TRANSITIONS
        //T3 - Check dist<=2
        //if (CheckDistanceLess(Utilities.DISTANCE_TO_FIGHT))
        if(Utilities.CheckDistanceLess(this.transform.position,enemy.transform.position,Utilities.DISTANCE_TO_FIGHT))
        {
            stateMachine.ChangeState(FIGHT_ENEMY);
            int damage = 10;
            enemy.GetComponent<Health>().TakeDamage(damage);
        }

        //T5 - Check Enemy dead, or lost from sight
        //if (EnemyDeadOrLostSight())
        if (Utilities.EnemyDeadOrLostSight(enemy.GetComponent<Health>(), enemy, this.gameObject, cosOfFOVover2InRAD))
        {
            stateMachine.CreateState(SEEW_WAY_POINT);

        }
    }

    /*private bool CheckDistanceLess(float v)
    {

        if (Vector3.Distance(this.transform.position, enemy.transform.position) <= v)
        {
            return true;
        }
        else
        {
            return false;
        }

    }*/

    private void DoChaseEnemy()
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, enemy.transform.position, maxSpeed * Time.deltaTime);
    }

    /*private bool EnemyDeadOrLostSight()
    {

        if (EnemyDead() || LostSight())
        {
            return true;
        }
        else
        {
            return false;
        }
    }*/

    private bool LostSight()
    {
        return !SeeEnemy();
    }

    private bool EnemyDead()
    {
        //TODO: Add a Health.cs script to the enemy with a public method IsAlive
        Health health = enemy.GetComponent<Health>();
        bool alive = health.IsAlive();
        return !alive;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        for (int i = 0; i < waypoints.Length; i++)
        {
            int i1 = (i + 1) % waypoints.Length;
            Gizmos.DrawLine(waypoints[i].position, waypoints[i1].position);


        }

        Gizmos.color = Color.black;
        Vector3 from = this.transform.position;
        Vector3 to = this.transform.position + this.transform.forward * 15;
        //Vector3 fovMinus=Vector3.RotateTowards(fro)
        Gizmos.DrawLine(from, to);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(waypoints[nextWaypointIndex].position, 1.2f);

        

    }
}
