using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiverController : MonoBehaviour {

	public Receiver stats;

    [Header("Body Parts")]
	public SpriteRenderer head;
	public SpriteRenderer body;
	public SpriteRenderer leftArm;
	public SpriteRenderer rightArm;
	public SpriteRenderer leftFoot;
	public SpriteRenderer rightFoot;
	public CircleCollider2D hands;
	public SpriteRenderer ball;
	public SpriteRenderer indicator;


	public float tightRoute = 0.2f; // how close he must get to the next point

	public Queue<Vector2> route = new Queue<Vector2>();

	private Animator animator;
	private Rigidbody2D rb;

	private Vector2 targetSpot = Vector2.zero;
    private GameplayManager gpm;

    //temporary for gizmos
    Vector2 direc = Vector2.zero;
    Vector2 tb = Vector2.zero;
    Vector2 sv = Vector2.zero;

	//audio
	public AudioClip catchSound;
	public AudioClip tackledSound;
	public AudioClip explodeSound;

    //state machine
    public enum State
    {
        IDLE,
        RUNNING_ROUTE,
        BALL_IN_AIR,
        SCORING,
        HAS_SCORED
    };
    public State curState = State.IDLE;

    // Use this for initialization
    void Start () {
		animator = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody2D> ();
        gpm = GameplayManager.instance;
        //qb = gpm.qb;
		//ez = EndzoneScript.instance;


        //configure player
        stats = GameManagerScript.instance.receivers[GameManagerScript.instance.startingReceiverIdx];
        hands.radius = stats.range/ Player.rangeDiv;
        SetUpSprites();

		//default target spot makes them run for the endzone
		targetSpot = GetEndzoneSpot();

	}
	
	// Update is called once per frame
	void FixedUpdate () {

        //float speed = stats.speed - 10f; // change could be a variable
        Vector2 direction = targetSpot - rb.position;

        switch (curState)
        {
            case State.IDLE:
            case State.HAS_SCORED:
                break;
            case State.SCORING:
                //GetTargetSpot(direction);
                //Move(direction);
                //AvoidDefenders();
                //break;
            case State.RUNNING_ROUTE:
                GetTargetSpot(direction);
                Move(direction);
                break;
            case State.BALL_IN_AIR:
                //speed = GetBoostSpeed();
                //incorporates ball target
                Move(GetTrackingVector(direction));

                sv = GetTrackingVector(direction); //for gizmos
                direction.Normalize();//for gizmos
                direc = direction; //for gizmos
                break;
        }

        animator.SetFloat("speed", rb.velocity.SqrMagnitude());

    }

    void Move(Vector2 dir)
    {
        dir.Normalize();
        float rotateAmount = -Vector3.Cross(dir, transform.up).z;
        rb.AddTorque(rotateAmount * stats.agility / Player.agilityDiv);
        rb.AddForce(transform.up * (stats.speed-10f) / Player.speedDiv);
    }

    public void Reset()
    {
        ball.enabled = false;
        GetComponent<CircleCollider2D>().enabled = true;
        indicator.enabled = false;

        //empty route
        route = new Queue<Vector2>();
        //default target spot makes them run for the endzone
        targetSpot = GetEndzoneSpot();
        curState = State.IDLE;
        gameObject.SetActive(true);
        transform.rotation = Quaternion.identity;
    }

    void OnDrawGizmos(){
		Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direc);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, tb);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, sv);
	}

    /*//something to add later
    float GetBoostSpeed()
    {
        float distR = Vector2.Distance(qb.target, rb.position);
        float distB = Vector2.Distance(qb.target, qb.transform.position);
        float ballSpeed = qb.stats.strength * Quarterback.strengthMod;
        float idealSpeed = distR / distB * ballSpeed;

        //dependent on tracking stat
        float newSpeed = stats.tracking * idealSpeed + (100f - stats.tracking) * (stats.speed-10f) / 100f;
        return Mathf.Clamp(newSpeed, 0f, stats.speed);
    }
    */


    Vector2 GetTrackingVector(Vector2 dir)
    {
        //when ball is in air, use tracking stat to go after it
        Vector2 toBall = gpm.qb.target - rb.position; 
        toBall.Normalize();
        tb = toBall;//for gizmos
        GetTargetSpot(dir); //need to keep updating target spot even after throw
        dir.Normalize();
        return (stats.tracking * toBall + (100f - stats.tracking) * dir) / 100f;

    }

    void GetTargetSpot(Vector2 d){   //either get from route or get endzone spot
        if (route.Count > 0)
        {
            if (d.magnitude < tightRoute)
            {
                targetSpot = route.Dequeue();
            }
        }
        else //route.count gets cleared when he catches ball
        {
            targetSpot = GetEndzoneSpot();
        }
    }

	public void CatchBall(FootballScript football){
        //print ("caught");

        gpm.SlowTime(0.2f);
		animator.SetTrigger ("catch");
        GameManagerScript.instance.PlaySound(catchSound);

		//targetSpot = GetEndzoneSpot(); //run to endzone

        route.Clear();

		football.GetCaught ();

		ball.enabled = true;
		indicator.enabled = true;

		gpm.qb.crosshair.SetActive (false);

        //maybe move this to gpm.Reception()
        gpm.SetDefendersTarget(transform);
        gpm.Reception();

        //ez.doEventText ("recept", transform.position);
    }


    #region Tackle
    public bool AttemptTackle(int wrapUp) { //called by defender when attempting a tackle
    Handheld.Vibrate ();
		int escape = UnityEngine.Random.Range (0, stats.breakout);
		int tackle = UnityEngine.Random.Range (0, wrapUp);
		if (tackle > escape) {
            GetTackled();
			return true;
		} 
        return false;
	}

	void GetTackled(){
		GetComponent<CircleCollider2D> ().enabled = false;
        //ez.playOver (false, 2.5f);
//		print("tackled");
		GameManagerScript.instance.PlaySound(tackledSound);

        //event where receiver was tackled with info about whether or not field goal line was beat
        gpm.ReceiverTackled(transform.position.y > gpm.FGL.position.y);
		//if (transform.position.y > gpm.FGL.position.y) { //if in field goal range
        //    print("field goal!!!");
        //    //ez.UpdateTextCelebrate("Fieldgoal!");
        //    //ez.changeScore(3);//kick a field goal
        //}else{
        //    print("tackled!");
        //    //ez.UpdateTextCelebrate("Tackled!");
        //}
		animator.SetTrigger ("getTackled"); //animation makes him explode
		
	}

	public void Disappear(){

		Handheld.Vibrate ();
		GameObject poof = Instantiate (stats.poofPrefab, transform.position, Quaternion.identity);
        GameManagerScript.instance.PlaySound(explodeSound);
		poof.transform.parent = null;

        gameObject.SetActive(false);
		//Destroy (gameObject);
	}
	#endregion

	void SetUpSprites(){
		if(stats != null){
			head.sprite = stats.head;
			body.sprite = stats.body;
			leftArm.sprite = stats.arm;
			rightArm.sprite = stats.arm;
			leftFoot.sprite = stats.leg;
			rightFoot.sprite = stats.leg;
			ball.enabled = false;
			indicator.enabled = false;
			indicator.color = stats.color;

		}else{
			Debug.LogWarning("Receiver has no stats");
		}
	}

	Vector2 GetEndzoneSpot(){
        if(gpm != null)
        {
            return new Vector2(transform.position.x, gpm.GL.position.y);//ez.transform.position.y);
        }
        print("gpm was null");
        return Vector2.zero;
	}

	public void SetRoute(List<Vector3> routePoints){
		route.Clear ();
		for (int i = 0; i < routePoints.Count; i++) {
			route.Enqueue(routePoints[i]);
		}
		targetSpot = route.Dequeue ();
	}


    void AvoidDefenders()
    {
        Transform closestThreat = FindClosestDefender();
        Vector2 avoidDirection = (transform.position - closestThreat.position).normalized;
        rb.AddForce(avoidDirection * stats.agility / Player.agilityDiv);
    }

    Transform FindClosestDefender()
    {
        Transform closestDef = null;
        float shortestDist = Mathf.Infinity;
        foreach (DefenderController d in gpm.defenders)
        {
            float sqrDist = Vector2.SqrMagnitude(d.transform.position-transform.position);
            if (sqrDist < shortestDist)
            {
                shortestDist = sqrDist;
                closestDef = d.transform;
            }
        }
        return closestDef;
    }

    //	void avoidDefenders (){
    //		Transform closestThreat = findClosestDefender ();
    //
    //		if (closestThreat != null) {
    //			if (Vector2.Distance (closestThreat.position, transform.position) < 1f) {
    //				Vector2 avoidDirection = (transform.position - closestThreat.transform.position).normalized;
    //
    //				if (avoidDirection.y > 0f) {//if hes already above defender
    //					avoidDirection.y = 1f - avoidDirection.y;  //stay above or just get vertical
    //					avoidDirection.x = 0f;
    //				} else {
    //					if (hasBall) {//go score and avoid defenders
    //						if (avoidDirection.x < 0f) { //if hes on the left of defender
    //							avoidDirection.x = -1f; //go left
    //						} else {
    //							avoidDirection.x = 1f; //go right
    //						}
    //						avoidDirection.y = 0f;
    //					} else {      //run across the field faster in direction hes going and get underneath defenders
    //						if (rb.velocity.x > 0) {
    //							avoidDirection.x = 1f;
    //						} else {
    //							avoidDirection.x = -1f;
    //						}
    //						avoidDirection.y = -1f - avoidDirection.y; //go down faster the closer he is to defender
    //
    //					}
    //				}
    //
    //
    //				rb.AddForce (avoidDirection * stats.agility * 10f);
    //
    //			}
    //		}
    //	}
    //
    //	Transform findClosestDefender (){
    //		float shortestDistance = Mathf.Infinity;
    //		GameObject closestDefender = null;
    //		foreach (GameObject d in defenders) {
    //			float currentDistance = Vector2.Distance (d.transform.position, transform.position);
    //			if (currentDistance < shortestDistance) {
    //				shortestDistance = currentDistance;
    //				closestDefender = d;
    //				//				print (closestDefender);
    //			}
    //		}
    //
    //		return closestDefender.transform;
    //	}
}
