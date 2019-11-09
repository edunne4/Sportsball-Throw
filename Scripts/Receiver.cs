using UnityEngine;

[CreateAssetMenu(fileName = "New Receiver", menuName = "Player/Receiver")]
public class Receiver : Player {

    [Range(0, 100)]
	public int speed = 50;          //how fast he is at running
    [Range(0, 100)]
    public int agility = 50;           //how quickly he can turn
    [Range(0, 100)]
    public int range = 50;       //how big his hands collider gets
    [Range(0, 100)]
    public int breakout = 50;          //chance of escaping a tackle
    [Range(0, 100)]
    //public int concentration = 0;  //nothing right now
    public int tracking = 0;        //how well the receiver goes after the ball when it's in the air
    [Range(0, 100)]
    public int hands = 50;        //how good at catching the ball
    //[Range(0, 100)]

    //public bool isOwned = false;

    public GameObject poofPrefab;
}
