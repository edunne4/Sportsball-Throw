using UnityEngine;
//using UnityEngine.EventSystems;

public class QuarterbackController : MonoBehaviour {

	public Quarterback stats;

    [Header("Body Parts")]
    public SpriteRenderer head;
	public SpriteRenderer body;
	public SpriteRenderer leftArm;
	public SpriteRenderer rightArm;
	public SpriteRenderer leftFoot;
	public SpriteRenderer rightFoot;
	public SpriteRenderer indicator;

    [Header("Other Stuff")]
    private Vector2 direction = Vector2.zero;
	public GameObject footbalPrefab;
	public GameObject currentBall = null;
    [HideInInspector]
	public Vector2 qbPos;
	public Vector2 target = Vector2.zero;
	//public bool hasThrown = false;
	//public bool hasHiked = false;
	private Touch touch;

	private Animator animator;
	//public int screenBuffer = 25;
	//private Transform throwArm;

	public GameObject crosshair;
	//private EndzoneScript endzone;
	//public GameObject hikeButton;
    //GameObject currentReceiver = null;
    private GameManagerScript gm;
    private GameplayManager gpm;




	//audio
	public AudioClip throwSound;

	//#region Singleton
	//public static QuarterbackController instance;
	//void Awake(){
	//	if (instance != null) {
	//		print ("More than one Quarterback!!");
	//		return;
	//	}
	//	instance = this;
	//}
	//#endregion

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		//audioSource = GetComponent<AudioSource> ();
		//endzone = EndzoneScript.instance;

        stats = GameManagerScript.instance.QBs[GameManagerScript.instance.startingQBIdx];

        qbPos = Camera.main.WorldToScreenPoint(transform.position);

        gm = GameManagerScript.instance;
        gpm = GameplayManager.instance;
        SetUpSprites();

		//GetSetForNextPlay ();

	}
	

	void Update () {

        if (gpm.curState == GameplayManager.GameState.HIKED)
        {
            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        TimeControl.StartSlowMotion(stats.quickThinking / Quarterback.QTDiv);
                        direction = Vector2.zero;

                        crosshair.SetActive(true);
                        target = Camera.main.ScreenToWorldPoint(touch.position);
                        crosshair.transform.position = target;
                        animator.SetBool("aiming", true);
                        break;
                    case TouchPhase.Moved:
                        //position of crosshair
                        target = Camera.main.ScreenToWorldPoint(touch.position);
                        crosshair.transform.position = target;
                        break;
                    case TouchPhase.Ended:
                        TimeControl.EndSlowMotion();
                        animator.SetBool("aiming", false);  //this throws ball
                        //direction must be global so that the throw ball function can
                        //access it when called from the animation
                        direction = touch.position - qbPos;


                        //animation controller throws ball

                        target = Camera.main.ScreenToWorldPoint(touch.position);
                        crosshair.transform.position = target;
                        break;
                }
            }
        }

        /*
		animator.SetBool ("hasThrown", hasThrown);
		//aim and throw ball

		if (!hasHiked) {
			return;
		}

		if (!hasThrown) {
			if (Input.touchCount > 0) {
				touch = Input.GetTouch (0);

                //if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                //{
                    if (touch.phase == TouchPhase.Began)
                    {
                        TimeControl.StartSlowMotion(stats.quickThinking / Quarterback.QTDiv);
                        direction = Vector2.zero;

                        crosshair.SetActive(true);
                        target = Camera.main.ScreenToWorldPoint(touch.position);
                        crosshair.transform.position = target;
                        animator.SetBool("aiming", true);
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        //position of crosshair
                        target = Camera.main.ScreenToWorldPoint(touch.position);
                        crosshair.transform.position = target;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        TimeControl.EndSlowMotion();
                        animator.SetBool("aiming", false);  //this throws ball

                        //direction must be global so that the throw ball function can
                        //access it when called from the animation
                        direction = touch.position - qbPos;


                        //animation controller throws ball

                        target = Camera.main.ScreenToWorldPoint(touch.position);
                        crosshair.transform.position = target;
                    }
                //}
		
			}
		}
		*/
    }

	public void ThrowFootball(){
		Quaternion ballRotation = Quaternion.Euler (0, 0, Mathf.Atan2(touch.position.y - qbPos.y,
			touch.position.x - qbPos.x) * Mathf.Rad2Deg - 90f);
		//print ("ball rotation: " + ballRotation);
		//throwArm.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
		gm.PlaySound (throwSound);
		currentBall = Instantiate (footbalPrefab, transform.position, ballRotation);
        currentBall.GetComponent<Rigidbody2D> ().AddForce (stats.strength * Quarterback.strengthMod * direction.normalized);
		currentBall.GetComponent<FootballScript> ().ThrowBall(touch.position);
		//hasThrown = true;
        animator.SetBool("hasThrown", true);
        indicator.enabled = false;

        gpm.BallThrown();
		//endzone.setDefenders (true); //tell defenders they can move if they aren't already

        //GameplayCanvas.instance.pauseButton.SetActive(true); //pause button goes off when hiked, comes back on after throw
	}

    #region setup
    //   public void GetSetForNextPlay (){
    //	//print ("setting up");
    //	hasThrown = false;
    //       //validTouch = true;
    //       indicator.enabled = true;
    //	crosshair.SetActive(false);
    //	if (currentBall != null) {
    //		Destroy (currentBall);
    //	}

    //	GameObject[] currentReceivers = GameObject.FindGameObjectsWithTag ("Receiver");
    //	foreach(GameObject r in currentReceivers){ 
    //		Destroy(r);
    //	}
    //	//throwArm.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;

    //	endzone.spawnDefenders ();
    //	PathCreator currentPath = GameObject.FindGameObjectWithTag ("Path").GetComponent<PathCreator>();
    //	currentPath.removePath ();

    //       //choose fieldgoal position
    //	Vector2 fglPos = new Vector2 (0f, Random.Range (-1f, 0.5f));
    //	Transform fgl = GameObject.FindGameObjectWithTag ("FGL").transform;
    //	fgl.localPosition = fglPos;

    //       //reset ball
    //	hasHiked = false;
    //	hikeButton.SetActive (true);

    //	spawnReceiver ();
    //}


    //	void spawnReceiver(){
    //		//print ("spawn receiver");
    //		//direction
    //		float dirX = 0;
    //		float dirY = Random.Range(0f, 0.8f);


    //		//position
    //		float x = 0;

    //		x = Random.Range (0, Screen.width);
    //		if (x > Screen.width/2) {
    //			//rightside
    ////			x = Screen.width + screenBuffer;
    //			dirX = -Random.Range(0.2f, 1f);
    //		} else {
    //			//leftside
    ////			x = -screenBuffer;
    //			dirX = Random.Range(0.2f, 1f);
    //		}
    //		//float y = Random.Range(Screen.height/6, Screen.height/3);
    //		float y = qbPos.y;
    //		Vector2 startSpot = Camera.main.ScreenToWorldPoint(new Vector2 (x, y));


    //		Vector2 direction = new Vector2 (dirX, dirY).normalized;


    //		Instantiate (receiverPrefab, startSpot, Quaternion.identity);

    ////		currentReceiver.GetComponent<ReceiverController> ().direction = direction;
    //}

    //	void spawnReceivers(int numR){
    //		//float offset1 = Random.Range
    //		Vector2 spot1 = new Vector2(transform.position.x - 2.5f, transform.position.y);
    //		Vector2 spot2 = new Vector2(transform.position.x + 2.5f, transform.position.y);

    //		Instantiate (receiverPrefab, spot1, Quaternion.identity);
    //		Instantiate (receiverPrefab, spot2, Quaternion.identity);
    ////		for (int i = 0; i < numR; i++) {
    ////			spawnReceiver ();
    ////		}
    //}
    public void Reset()
    {
        if (animator != null)
        {
            animator.SetBool("hasThrown", false);
        }
        indicator.enabled = true;
        if (currentBall != null)
        {
            Destroy (currentBall);
        }
        qbPos = Camera.main.WorldToScreenPoint(transform.position);
    }


    void SetUpSprites(){
		if(stats != null){
			head.sprite = stats.head;
			body.sprite = stats.body;
			leftArm.sprite = stats.arm;
			rightArm.sprite = stats.ballArm;
			leftFoot.sprite = stats.leg;
			rightFoot.sprite = stats.leg;
			indicator.color = stats.color;
			indicator.enabled = true;
		}else{
			Debug.LogWarning("Quarterback has no stats");
		}
	}
    #endregion
}
