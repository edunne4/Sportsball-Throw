using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManagerScript : MonoBehaviour
{

    //player info
    public int startingReceiverIdx;
    public Receiver[] receivers;
    public bool[] myReceivers;

    public int startingQBIdx;
    public Quarterback[] QBs;
    public bool[] myQBs;


    //level stuff
    public int tournUnlocked = 0;
    public int teamUnlocked = 0;
    public int currentTournIdx = 0;
    public int currentTeamIdx = 0;
    public Tournament[] tournaments;

    [HideInInspector]
    public int money = 0;

	//audio
	public AudioSource audioSource;
	public AudioClip buttonSound;

	#region Singleton
	public static GameManagerScript instance;
	void Awake () {

        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            print("More than one GameManager!!");
            Destroy(gameObject);
        }


	}
	#endregion

	void Start(){
		
		audioSource = GetComponent<AudioSource> ();

        //LoadGame();
    }


    public void PlaySound(AudioClip sound = null)

    {
        if (sound == null)
        {
            sound = instance.buttonSound;
        }
        if (instance.audioSource != null)
        {
            instance.audioSource.PlayOneShot(sound, 0.5f); //must be instance to work!!!
        }

    }


	public void LoadNewScene(string scene){
        //playSound(buttonSound);

        SceneManager.LoadScene (scene);
		//playingGame = true;

	}

	public void endGame(int moneyMade){
		////GameplayCanvas c = GameplayCanvas.instance;
		//c.HUD.SetActive (false);
        ////if you won, display win menu, otherwise game over menu
        //if (score > teams[currentLevel].scoreToBeat){
        //    //currentLevel++; //increase current level so that when next level button is clicked it goes to next
        //    if (currentLevel + 1 >= levelUnlocked) //if on the most unlocked level
        //    {
        //        levelUnlocked = currentLevel + 1; //next level is unlocked
        //    }
        //    c.WM.SetActive(true);
        //}
        //else{
        //    c.GOM.SetActive(true);
        //}
        //c.scoreText.text = "Final Score:\n" + 
        //    "Your Team: "+score+ "   \n" + 
        //    teams[currentLevel].name +": "+ teams[currentLevel].scoreToBeat;
        //c.rewardText.text = "Reward: $" + moneyMade;
        //c.results.SetActive(true);
        //money += moneyMade;

        ////SceneManager.LoadScene ("PlayAgain");
        //SaveGame();
    }


    public void SaveGame()
    {
        SaveSystem.SaveGame(this);
    }

    public void LoadGame()
    {
        PlayerData data = SaveSystem.LoadGame();

        if(data != null)
        {
            tournUnlocked = data.tournUnlocked;
            teamUnlocked = data.teamUnlocked;
            money = data.money;

            startingQBIdx = data.startingQBIdx;
            startingReceiverIdx = data.startingReceiverIdx;

            myQBs = data.QBsOwned;
            myReceivers = data.receiversOwned;
        }

    }

    internal Team GetNextTeam()
    {
        //increment current team
        currentTeamIdx++;
        //if last team in tournament, go to next tournament
        if(currentTeamIdx >= tournaments[currentTournIdx].teams.Length)
        {
            currentTournIdx++;
            currentTeamIdx = 0;
            //if last tournament, return null and go to main menu
            if (currentTournIdx >= tournaments.Length)
            {
                currentTournIdx = 0;
                print("You beat the game yay");
                LoadNewScene("Main Menu Scene");
            }
        }
        return tournaments[currentTournIdx].teams[currentTeamIdx];
    }

    internal void UpdateUnlocked()
    {
        //check if current tournament is most unlocked tournament;
        if(currentTournIdx >= tournUnlocked)
        {
            //check if current team is most unlocked team
            if(currentTeamIdx >= teamUnlocked)
            {
                //if team unlocked is not the last team, increment it
                if(teamUnlocked < tournaments[tournUnlocked].teams.Length - 1)
                {
                    teamUnlocked++;
                    //if tourn unlocked is not last tournament
                    if (tournUnlocked < tournaments.Length - 1)
                    {
                        //increment tournament and reset team
                        tournUnlocked = currentTournIdx + 1;
                        teamUnlocked = 0;
                    }
                }
            }
        }
    }
}
