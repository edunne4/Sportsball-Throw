using UnityEngine;
using TMPro;
using System;

public class GameplayCanvas : MonoBehaviour {

	public GameObject HUD;
	public GameObject loseMenu;
    public GameObject winMenu;
    public GameObject hikeButton;
    public GameObject pauseButton;
    public TextMeshProUGUI myScoreText;
    public TextMeshProUGUI scoreToBeatText;
    public TextMeshProUGUI downText;
    //public TextMeshProUGUI rewardText;
    //public GameObject results;

    //private GameManagerScript gm;

    #region Singleton
    public static GameplayCanvas instance;
	void Awake(){
		if (instance == null) {
            instance = this;

		}else if(instance != this)
        {
            print("More than one Gameplay Canvas!!");
            Destroy(gameObject);
        }

    }
    #endregion

    // Use this for initialization
    void Start()
    {
        //gm = GameManagerScript.instance;
    }

    internal void ShowLoseMenu()
    {

        loseMenu.SetActive(true);
    }

    internal void ShowWinMenu()
    {
        winMenu.SetActive(true);
    }




    //public void NextLevel()
    //{

    //    if (gm.currentLevel + 1 < gm.teams.Length)
    //    {
    //        gm.currentLevel++;
    //        gm.LoadNewScene("Gameplay");
    //    }
    //}

    //public void QuitToMain()
    //{
    //    print("quit to main");
    //    Time.timeScale = 1f;
    //    gm.LoadNewScene("MainMenu2");
    //}

    //public void pauseGame()
    //{
    //    Time.timeScale = 0f;
    //}

    //public void resumeGame()
    //{
    //    Time.timeScale = 1f;
    //}
}
