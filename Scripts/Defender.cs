using UnityEngine;

[CreateAssetMenu(fileName = "New Defender", menuName = "Player/Defender")]
public class Defender : Player {

	public int speed = 50;
	public int agility = 50;
	public int range = 50;
	public int wrapUp = 50;
	//public int concentration = 0;
	public int coverage = 50;
    public int hands = 50;        //how good at catching the ball


    public GameObject poofPrefab;


}
