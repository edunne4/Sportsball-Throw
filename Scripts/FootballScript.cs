using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootballScript : MonoBehaviour {

	private Animator animator;
    [HideInInspector]
	public Rigidbody2D ball;
    //public ParticleSystem trailParticles;
    //private ParticleSystem particles;
    private TrailRenderer trail;
	public ParticleSystem dirtParticles;

    //related to calculating ball size/catchability
	private Vector2 midPoint;
	private Vector2 target = new Vector2(100f, 100f);
    private Vector2 startPos = Vector2.zero;
	private float throwDist;
	private float throwRatio = 1f;
	private float fullDist = 1f;
    private readonly float maxGrow = 2f; //only thing to change
    private float frac = 1f;

    private float growFactor = 1f;
	public float howLowToCatch = 1.01f;
	public bool inAir = true;
	public int bounceForce = 50; // how fast ball spins on bounce
    //public GameObject endzone;
    private GameplayManager gpm;

	//audio
	public AudioClip bounceSound;

	void Start () {
		ball = GetComponent<Rigidbody2D> ();
		animator = GetComponent<Animator> ();
        //create particles as child
        //particles = Instantiate (trailParticles, transform.position, Quaternion.identity);
        //particles.transform.parent = transform;
        trail = GetComponent<TrailRenderer>();
        gpm = GameplayManager.instance;

        //endzone = EndzoneScript.instance.gameObject;//GameObject.FindGameObjectWithTag ("Endzone");

		//print(ball.velocity.magnitude);
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		animator.SetFloat("Speed", ball.velocity.magnitude);

        HeightControl(); //determines scale of ball and whether or not it's catchable

			
	}

	public void ThrowBall(Vector2 touchPoint){
		Vector2 fullDistPoint = Camera.main.ScreenToWorldPoint( new Vector2(touchPoint.x, Screen.height));
		target = Camera.main.ScreenToWorldPoint (touchPoint);
        //target = touchPoint;
        //		print ("target: " + target);
        //		print ("touchpoint: " + touchPoint);

        gpm = GameplayManager.instance;
        startPos = gpm.qb.transform.position;
		fullDist = Vector2.Distance (startPos, fullDistPoint);

//		print ("touchPoint: " + touchPoint);
//		print ("qbPos: " + qbPos);
//		print("throwDist: " + throwDist);
//		print("fullDist: " + fullDist);
		midPoint = (target + startPos) / 2;
		//print ("midPoint: " + midPoint);
		throwDist = Vector2.Distance(startPos, target);

        throwRatio = 2f * throwDist / fullDist ;
        //throwRatio = 2f * Mathf.Pow(throwDist, 2) / Mathf.Pow(fullDist, 2);   ///fix tthis
        //frac = (maxGrow - 1f/throwRatio) / Mathf.Pow(throwDist/2f, 2);
        frac = (maxGrow - 1f/throwRatio) / Mathf.Pow(throwDist / 2f, 2);
        //print (staticDist);
    }

	void HeightControl(){
		float distFromMid = Vector2.Distance((Vector2)transform.position, midPoint);

        //if (distFromMid != 0) {
        //	//      initial distance to midpoint        ratio of throw to farthest throw
        //	growFactor = Mathf.Pow((throwDist / 2 / distFromMid), 2f) * throwRatio;
        //}
        if (inAir)
        {
            //max          //fraction              //x^2
            growFactor = throwRatio * (maxGrow - frac * Mathf.Pow(distFromMid, 2));
            //print(growFactor);

            growFactor = Mathf.Clamp(growFactor, 1f, maxGrow);

            //check if flight is over
            if (Vector2.Distance(startPos, transform.position) > throwDist + 0.3f)
            {
                Bounce();
            }
        }
        else{
            growFactor = 1f;
        }
        //print("grow factor: " + growFactor);

        //set the scale
        Vector3 newScale = new Vector3(growFactor, growFactor, 1f);
        transform.localScale = newScale;

        //scale trail
        trail.startWidth = growFactor * 0.25f;

		//check if its too high
        GetComponent<CircleCollider2D> ().enabled = growFactor < howLowToCatch && inAir;

    }

	void Bounce(){
		inAir = false;
		ball.drag = 3f;
        GameManagerScript.instance.PlaySound(bounceSound);//audioSource.PlayOneShot (bounceSound);
        transform.localScale = Vector3.one;

        Instantiate (dirtParticles, transform.position, Quaternion.identity);
		GetComponent<CircleCollider2D> ().enabled = false;

		float groundForce = (Random.value < 0.5f) ? bounceForce : (-1 * bounceForce);

		ball.AddTorque (groundForce, ForceMode2D.Impulse);

		//EndzoneScript.instance.playOver(false, 2); //ends game after 2 seconds

        trail.time = 0.5f; //make trail disappear quickly

        gpm.BallHitGround();
	}

//	IEnumerator endGameCo(){
//		yield return new WaitForSeconds (2f);
//
//		endzone.GetComponent<EndzoneScript> ().failToScore (false);
//	}

	public void GetCaught(){
        //particles.Stop ();
        //particles.transform.parent = null;
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        ball.velocity = Vector2.zero;

		Destroy(gameObject, 1f);
	}

    public IEnumerator GetKnockedDown(Vector2 direction)
    {
        ball.velocity = direction;
        float knockForce = (Random.value < 0.5f) ? bounceForce : (-1 * bounceForce);

        ball.AddTorque(knockForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.15f);
        Bounce();
    }

	void OnTriggerEnter2D(Collider2D other){
		Transform player = other.transform.parent;
        if (player == null)
        {
            print("ball hit something else, not a player: "+player);
        }
        else
        {
            if (player.CompareTag("Receiver") && inAir)
            {
                inAir = false;
                player.GetComponent<ReceiverController>().CatchBall(GetComponent<FootballScript>());
            }
            else if (player.CompareTag("Defender") && inAir)
            {
                inAir = false;
                player.GetComponent<DefenderController>().CatchBall(GetComponent<FootballScript>());
            }
        }
	}
}
