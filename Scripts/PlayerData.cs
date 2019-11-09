using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData {

    public int tournUnlocked = 0;
    public int teamUnlocked = 0;
    public int money;
    public int startingReceiverIdx;
    public int startingQBIdx;

    public bool[] receiversOwned;
    public bool[] QBsOwned;


    public PlayerData (GameManagerScript gm)
    {
        tournUnlocked = gm.tournUnlocked;
        teamUnlocked = gm.teamUnlocked;
        money = gm.money;
        startingQBIdx = gm.startingQBIdx;
        startingReceiverIdx = gm.startingReceiverIdx;

        //if these dont work, loop through gm.myRecievers adding each one individually
        receiversOwned = gm.myReceivers;
        QBsOwned = gm.myQBs;
    }
}
