using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderController : MonoBehaviour {

	public Defender stats;

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

    //private bool isScoring = false;
    //public bool isMoving = false;
    [HideInInspector]
    public Transform targetReceiver;// = null;
    private Vector2 target = Vector2.zero;

    private Animator animator;
	private Rigidbody2D rb;

    //other objects
    private GameManagerScript gm;
    private GameplayManager gpm;

    //state machine
    public enum State
    {
        IDLE,
        FOLLOWING,
        GOING_TO_BALL,
        GOING_TO_TACKLE,
        SCORING
    };
    public State curState = State.IDLE;

    //audio
    public AudioClip catchSound;
    public AudioClip knockDownSound;
	public AudioClip explodeSound;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody2D> ();
        gm = GameManagerScript.instance;
        gpm = GameplayManager.instance;
		

        //configure player
        hands.radius = stats.range/Player.rangeDiv;
        SetUpSprites();
		
	}

	// Update is called once per frame
	void FixedUpdate () {
		Vector2 direction = target - rb.position;


        switch (curState)
        {
            case State.IDLE:
                target = transform.position;
                break;
            case State.FOLLOWING:
                target = targetReceiver.position;
                //if not too close, go after them
                if (direction.magnitude > -stats.coverage / 100f + 1f)
                {
                    Move(direction);
                }
                break;

            case State.GOING_TO_BALL:
                target = gpm.qb.target;
                Move(direction);
                break;
            case State.GOING_TO_TACKLE:
                target = targetReceiver.position;
                Move(direction);
                break;
            case State.SCORING:
                target = gpm.otherEndzone.position;
                Move(direction);
                break;
        }

        animator.SetFloat("speed", rb.velocity.magnitude);

    }


    void Move(Vector2 dir)
    {
        dir.Normalize();
        float rotateAmount = -Vector3.Cross(dir, transform.up).z;
        rb.AddTorque(rotateAmount * stats.agility / Player.agilityDiv);
        rb.AddForce(transform.up * stats.speed / Player.speedDiv);
    }

    #region catch
    public void CatchBall(FootballScript football){
    	//print ("caught");
    	gpm.SlowTime(0.2f);

        if (Random.Range(0, 100) < stats.hands/2f)
        {//successful catch
            animator.SetTrigger("catch");
            gm.PlaySound(catchSound);

            football.GetCaught();
            ball.enabled = true;
            indicator.enabled = true;
            gpm.Interception();
            gpm.SetDefendersTarget(gpm.otherEndzone);

            gpm.qb.crosshair.SetActive(false);
           

           //endzone.doEventText("inter", transform.position);
           //endzone.playOver(true, 3);
        }else{//knocked ball down
               //add force to ball in random direction
               animator.SetTrigger("catch");
               gm.PlaySound(knockDownSound);
               StartCoroutine(football.GetKnockedDown(new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f))) );
               //endzone.doEventText("knock", transform.position);
           }

    }


    #endregion

    //detect receivers nearby
    void OnTriggerStay2D(Collider2D other){
        //if (gpm.curState != GameplayManager.GameState.PLAY_NOT_STARTED)
        //{
        if (other.CompareTag("Receiver"))
        {
            if (other.GetComponent<ReceiverController>().curState == ReceiverController.State.RUNNING_ROUTE)
            {//.hasScored && qb.hasHiked){
                curState = State.FOLLOWING;
                targetReceiver = other.transform;
            }
        }
        //}
	}

    //tackle receiver
	void OnCollisionStay2D(Collision2D other){
		//tackle stuff
		if(other.gameObject.CompareTag("Receiver")){
			//tackle receiver
			ReceiverController r = other.gameObject.GetComponent<ReceiverController> ();

			if (r.curState == ReceiverController.State.SCORING) { 
            //if the receiver is scoring, tackle him
				animator.SetTrigger ("tackle");
				//print("I'll get him!!");
				bool success = r.AttemptTackle(stats.wrapUp);
				if (success) {
                    //play over
                    gpm.SetDefendersState(State.IDLE);
				} else { //get wrecked
                    Disappear();
				}
				//rb.mass = 100;
				//GetComponent<CircleCollider2D> ().enabled = false;
			} else if (curState == State.SCORING) {
                //else if defender is scoring, destroy receiver
				r.Disappear();
			}
		}
	}

    public void Disappear()
    {
        GameObject poof = Instantiate(stats.poofPrefab, transform.position, Quaternion.identity);
        GameManagerScript.instance.PlaySound(explodeSound);
        poof.transform.parent = null;

        gameObject.SetActive(false);
        //Destroy (gameObject);
    }



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
			Debug.LogWarning("Defender has no stats");
		}
	}
}

	
