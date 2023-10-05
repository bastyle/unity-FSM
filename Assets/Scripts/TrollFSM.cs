using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrollFSM : MonoBehaviour
{

    public TrollStates currentState = TrollStates.REALIGN_WAYPOINT;
    public GameObject enemy;
    public float maxSpeed = 2;//2m/s 

    public float FOVinDEG = 70;
    private float cosOfFOVover2InRAD;  //cut off value for visibility checks

    public Transform[] waypoints;
    public int nextWaypointIndex = 0;

    public float maxAngularSpeedInDegPerSec = 60; //deg/sec
    public float maxAngularSpeedInRadPerSec; //rad/sec
    private float maxAngularSpeedInRadPerFrame;
    void Start()
    {
        cosOfFOVover2InRAD = Mathf.Cos(FOVinDEG / 2f * Mathf.Deg2Rad);
        maxAngularSpeedInRadPerSec = maxAngularSpeedInDegPerSec * Mathf.Deg2Rad;


    }

    // Update is called once per frame
    void Update()
    {
        FSM();

    }

    private void FSM()
    {
        switch (currentState)
        {
            case TrollStates.REALIGN_WAYPOINT:
                HandleRealignWaypoint();
                break;
            case TrollStates.SEEKWAY_POINT:
                HandleSeekWaypoint();
                break;
            case TrollStates.CHASE_ENEMY:
                HandleChaseEnemy();
                break;
            case TrollStates.FIGH_TENEMY:
                HandleFightEnemy();
                break;
            default:
                //throw new Exception("current State is invalid");
                print("current State is invalid" + currentState);
                break;

        }
    }

    private void HandleFightEnemy()
    {
        //DEFAULT ACTION
        print("HandleFightEnemy");
        DoFightEnemy();

        //TRANSITION CHECKS
        //T5 - Enemy Dead or Lost Sight
        if (Utilities.EnemyDeadOrLostSight(enemy.GetComponent<Health>(), enemy, this.gameObject, cosOfFOVover2InRAD))
        {
            ChangeState(TrollStates.REALIGN_WAYPOINT);
        }
        //T6 - dit>2
        if (!Utilities.CheckDistanceLess(this.transform.position, enemy.transform.position, Utilities.DISTANCE_TO_FIGHT))
        {
            ChangeState(TrollStates.CHASE_ENEMY);
        }

    }

    private void DoFightEnemy()
    {
        //throw new NotImplementedException();
        int damage = UnityEngine.Random.Range(0, 100);
        enemy.GetComponent<Health>().TakeDamage(damage);
    }

    private void HandleChaseEnemy()
    {
        //DEFAULT
        print("HandleChaseEnemy");
        DoChaseEnemy();

        //Check TRANSITIONS
        //T3 - Check dist<=2
        if (Utilities.CheckDistanceLess(this.transform.position,enemy.transform.position,Utilities.DISTANCE_TO_FIGHT))
        {
            ChangeState(TrollStates.FIGH_TENEMY);
            int damage = 10;
            enemy.GetComponent<Health>().TakeDamage(damage);
        }

        //T5 - Check Enemy dead, or lost from sight
        //if (EnemyDeadOrLostSight())
        if(Utilities.EnemyDeadOrLostSight(enemy.GetComponent<Health>(), enemy, this.gameObject, cosOfFOVover2InRAD))
        {
            ChangeState(TrollStates.SEEKWAY_POINT);

        }
    }

    
   

    private void DoChaseEnemy()
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, enemy.transform.position, maxSpeed * Time.deltaTime);
    }

    private void HandleSeekWaypoint()
    {
        //DEFAULT ACTION
        print("HandleSeekWaypoint::::::::");
        DoSeekWaypoint();

        //CHECK FOR  TRANSITIONS
        //T4 - Waypoint Reached?
        //if (WaypointReached())
        if (Utilities.WaypointReached(this.transform.position, waypoints[nextWaypointIndex].position))
        {
            nextWaypointIndex = (nextWaypointIndex + 1) % waypoints.Length;
            ChangeState(TrollStates.REALIGN_WAYPOINT);
        }
        //T2 - SeeEnemy?
        //if (SeeEnemy())
        if (null != enemy && Utilities.SeeEnemy(this.transform.position, enemy.transform.position, this.transform.forward, cosOfFOVover2InRAD))
        {
            ChangeState(TrollStates.CHASE_ENEMY);
        }

    }



    private void DoSeekWaypoint()
    {

        this.transform.position = Vector3.MoveTowards(this.transform.position, waypoints[nextWaypointIndex].position, maxSpeed * Time.deltaTime);
    }

    private void HandleRealignWaypoint()
    {
        //DEFAULT
        print("HandleRealignWaypoint");
        DoRealign();

        //TRANSITIONS
        //T1 - Aligned?
        if(Utilities.IsAligned(this.transform.position, waypoints[nextWaypointIndex].position, this.transform.forward, 0.01f))
        {
            ChangeState(TrollStates.SEEKWAY_POINT);
        }
    }

    private void ChangeState(TrollStates newState)
    {
        currentState = newState;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            int i1 = (i + 1) % waypoints.Length;
            Gizmos.DrawLine(waypoints[i].position, waypoints[i1].position);


        }

        Gizmos.color = Color.cyan;
        Vector3 from = this.transform.position;
        Vector3 to = this.transform.position + this.transform.forward * 10;
        //Vector3 fovMinus=Vector3.RotateTowards(fro)
        Gizmos.DrawLine(from, to);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(waypoints[nextWaypointIndex].position, 1.2f);

    }
}
