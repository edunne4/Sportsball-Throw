using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GameplayManager : MonoBehaviour
{

    //game objects
    public SpriteRenderer footballField;
    public QuarterbackController qb;
    public ReceiverController receiver;
    public PathCreator pathCreator;
    public Transform LOS; //line of scrimmage
    public Transform GL; //goal line
    public Transform FGL; //field goal line
    public Transform otherEndzone; //where the defenders should run
    private GameManagerScript gm;
    private GameplayCanvas gpc;


    //GUI Stuff

    private readonly string[] downTexts = { "1st", "2nd", "3rd", "4th" };
    private readonly Color[] downColors = {new Color(0f, 0.75f, 0.1f, 1f), new Color(0.83f, 0.71f, 0f, 1f),
        new Color(1f, 0.53f, 0f, 1f), new Color(1f, 0f, 0f, 1f) }; //green, yellow, orange, red
    //text stuff
    public TextMeshProUGUI updateText;
    public TextMeshProUGUI eventText;
    public Animator canvasAnimator;



    //defender stuff
    public int MAX_DEFENDERS = 5;
    public DefenderController[] defenders;
    public GameObject defenderPrefab;
    public int edgeBuffer = 35;

    //audiio
    public AudioClip hikeSound;


    private Team currentTeam;

    private int score = 0;
    private int down = 1;
    private int moneyThisGame = 0;

    //game state machine
    public enum GameState
    {
        PLAY_NOT_STARTED,
        HIKED,
        BALL_IN_AIR,
        RECEIVER_HAS_BALL,
        PLAY_OVER
    }
    public GameState curState = GameState.PLAY_NOT_STARTED;

    #region Singleton
    public static GameplayManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            print("More than one GameplayManager!!");
            Destroy(gameObject);
        }

    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        ConfigureFieldAndCamera();
        gm = GameManagerScript.instance;
        gpc = GameplayCanvas.instance;
        currentTeam = gm.tournaments[gm.currentTournIdx].teams[gm.currentTeamIdx];
        SetUpNewGame();
    }


    private void ConfigureFieldAndCamera()
    {
        //size of sprite in world units (assuming scale is set to 1)
        Vector2 local_sprite_size = footballField.sprite.rect.size / footballField.sprite.pixelsPerUnit;

        //proportion of screen that sprite takes up
        Vector2 screen_size = 0.5f * local_sprite_size / Camera.main.orthographicSize;

        //orthographic size is height of camera, so to get width,  we divide the x by the aspect ratio idk why
        screen_size.x /= Camera.main.aspect;

        //scale should be proportion of field size to 
        footballField.transform.localScale /= screen_size.x;

        //height of field in world units
        float height = local_sprite_size.y * footballField.transform.lossyScale.y;
        //y position for camera
        float yPos = height * 0.5f - Camera.main.orthographicSize;
        //set camera's new positon
        Camera.main.transform.position = new Vector3(0f, yPos, -10f);
    }

    void SetUpNewGame()
    {
        down = 1;
        score = 0;
        //reset score and down texts
        gpc.downText.text = downTexts[down - 1];
        gpc.downText.color = downColors[down - 1];
        gpc.scoreToBeatText.text = currentTeam.scoreToBeat.ToString();
        gpc.myScoreText.text = score.ToString();

        moneyThisGame = 0;

        SetUpNextPlay();
    }

    void SetUpNextPlay()
    {
        PositionPlayers();
        SpawnDefenders();
        qb.crosshair.SetActive(false);
        pathCreator.line.enabled = false;
        pathCreator.line.positionCount = 0;
        pathCreator.gameObject.SetActive(true);

        //randomize field goal position, fix these bounds
        FGL.localPosition = new Vector2(0f, UnityEngine.Random.Range(-1f, 0.5f));


        gpc.hikeButton.SetActive(true);
        curState = GameState.PLAY_NOT_STARTED;
    }

    void PositionPlayers()
    {

        float scale = 0.75f;
        //qb position first
        qb.transform.position = LOS.position;
        qb.transform.localScale = new Vector3(scale, scale, 1f);
        qb.Reset();


        //receiver position
        //puts him on the right
        float screenX = UnityEngine.Random.Range(qb.qbPos.x + edgeBuffer, Screen.width-edgeBuffer);
        if(UnityEngine.Random.value < 0.5f)
        { //puts him on the left
            screenX = UnityEngine.Random.Range(edgeBuffer, qb.qbPos.x - edgeBuffer);
        }
        float screenY = Camera.main.WorldToScreenPoint(LOS.position).y;
        Vector2 receiverPos = Camera.main.ScreenToWorldPoint(new Vector2(screenX, screenY));

        receiver.transform.position = receiverPos;
        receiver.transform.localScale = new Vector3(scale, scale, 1f);
        receiver.Reset();
    }

    void SpawnDefenders()
    {
        //first, destroy any remaining defenders
        if (defenders.Length > 0)
        {
            foreach (DefenderController D in defenders)
            {
                Destroy(D.gameObject);
            }
        }
        //spawn new ones
        int numDefenders = UnityEngine.Random.Range(3, MAX_DEFENDERS + 1);
        //create new array with new number of defenders
        defenders = new DefenderController[numDefenders];


        float minY = LOS.position.y +1f;
        float maxY = GL.position.y -1f;
        float minX = Camera.main.ScreenToWorldPoint(new Vector2(edgeBuffer, 0f)).x;
        float maxX = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width - edgeBuffer, 0f)).x;

        for (int i = 0; i < numDefenders; i++)
        {
            float x = UnityEngine.Random.Range(minX, maxX);
            float y = UnityEngine.Random.Range(minY, maxY);

            Vector2 newPos = new Vector2(x, y);
            Vector2 dir = (Vector2)qb.transform.position - newPos;
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

            //int defenderIndex = Random.Range(0, possibleDefenders.Length);
            GameObject newDef = Instantiate(defenderPrefab, newPos, Quaternion.Euler(0, 0, ang));

            //create new defender stats and give them to the defender instance
            newDef.GetComponent<DefenderController>().stats = currentTeam.CreateMember();

            //add new defender to defender array
            defenders[i] = newDef.GetComponent<DefenderController>();

            //give it a random scale
            float scale = UnityEngine.Random.Range(0.75f, 1f);
            newDef.transform.localScale = new Vector3(scale, scale, 1f);

        }
    }

    # region State Machine Event Handlers
    public void HikePressed()
    {
        curState = GameState.HIKED;
        receiver.curState = ReceiverController.State.RUNNING_ROUTE;
        gm.PlaySound(hikeSound);
    }

    public void BallThrown()
    {
        curState = GameState.BALL_IN_AIR;
        receiver.curState = ReceiverController.State.BALL_IN_AIR;
        SetDefendersState(DefenderController.State.GOING_TO_BALL);
        gpc.pauseButton.SetActive(true);
    }

    public void BallHitGround()
    {
        curState = GameState.PLAY_OVER;
        receiver.curState = ReceiverController.State.IDLE;
        SetDefendersState(DefenderController.State.IDLE);

        //end play in some time
        StartCoroutine(PlayWillEndIn(2f));
    }

    public void Reception()
    {
        curState = GameState.RECEIVER_HAS_BALL;
        receiver.curState = ReceiverController.State.SCORING;
        SetDefendersState(DefenderController.State.GOING_TO_TACKLE);
    }

    public void Interception()
    {
        curState = GameState.PLAY_OVER;
        receiver.curState = ReceiverController.State.IDLE;
        SetDefendersState(DefenderController.State.SCORING); //tell defenders they can run now
        //end play in some time
        down = 5; //this is so the game ends immediately! maybe not the best way to handle interceptions
        StartCoroutine(PlayWillEndIn(3f));
    }

    public void ReceiverTackled(bool didGetFieldGoal)
    {
        curState = GameState.PLAY_OVER;
        receiver.curState = ReceiverController.State.IDLE;
        SetDefendersState(DefenderController.State.IDLE);
        //add to score if field goal achieved
        if (didGetFieldGoal)
            UpdateScore(3);
        
        //end play in some time
        StartCoroutine(PlayWillEndIn(2f));
    }

    public void Touchdown()
    {
        print("touchdown!");
        curState = GameState.PLAY_OVER;
        receiver.curState = ReceiverController.State.HAS_SCORED;
        SetDefendersState(DefenderController.State.IDLE);
        //add to score
        UpdateScore(7);
        //get touchdown money reward
        moneyThisGame += currentTeam.touchdownBonus;

        //end play in some time
        down--; //this is so that the down does not increase for a touchdown, maybe not the best way to handle this!
        StartCoroutine(PlayWillEndIn(3f));
    }


    public void SetDefendersState(DefenderController.State newState)
    {
        foreach (DefenderController defender in defenders)
        {
            defender.curState = newState;
        }
        
    }
    internal void SetDefendersTarget(Transform ballHolder)
    {
        foreach (DefenderController defender in defenders)
        {
            defender.targetReceiver = ballHolder;
        }
    }
    #endregion

    void UpdateScore(int addToScore)
    {
        score += addToScore;

        //update score text (move to gpc)
        gpc.myScoreText.text = score.ToString();
    }

    private IEnumerator PlayWillEndIn(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayHasEnded();
    }

    void PlayHasEnded()
    {
        //turn of path so buttons don't affect it
        pathCreator.gameObject.SetActive(false);

        //if game is over, handle that, else set up next play
        if (score > currentTeam.scoreToBeat)
        {
            WinGame();
        }
        else
        {
            if (down < 4)
            {
                SetUpNextPlay();
                down++;
                //update down text
                gpc.downText.text = downTexts[down-1];
                gpc.downText.color = downColors[down - 1];
            }
            else
            {
                //game over
                LoseGame();
            }
        }
    }

    void LoseGame()
    {
        gpc.ShowLoseMenu();
        gm.money += moneyThisGame;
    }
    void WinGame()
    {
        moneyThisGame += currentTeam.moneyReward;
        gm.money += moneyThisGame;
        gpc.ShowWinMenu();
        gm.UpdateUnlocked();
        //currentTeam = gm.GetNextTeam();
    }

    public void ReplayGame()
    {
        SetUpNewGame();
    }

    public void NextGame()
    {
        currentTeam = gm.GetNextTeam();
        SetUpNewGame();
    }


    public void SlowTime(float duration)
    {
        StartCoroutine(TimeControl.SlowDownCo(duration));
    }





    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }


}
