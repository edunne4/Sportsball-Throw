using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Team", menuName = "Team")]
public class Team : ScriptableObject {

    public string city = "";
    public string teamName = "";

    [Range(0, 100)]
    public int teamMin = 50;
    [Range(0, 100)]
    public int teamMax = 50;

    public int scoreToBeat = 7;
    public int moneyReward = 100;
    public int touchdownBonus = 7;


    public GameObject poofPrefab;

    public Sprite head;
    public Sprite body;
    public Sprite blackArm;
    public Sprite tanArm;
    public Sprite whiteArm;
    public Sprite leg;

    public Color color;

    public Defender CreateMember()
    {
        Defender newMember = (Defender)CreateInstance("Defender");

        newMember.head = head;
        newMember.body = body;
        float val = Random.value;
        if (val < 1f/3f)
        {
            newMember.arm = blackArm;
        }else if (val < 2f/3f)
        {
            newMember.arm = tanArm;
        }
        else
        {
            newMember.arm = whiteArm;
        }
        newMember.leg = leg;

        newMember.color = color;
        newMember.poofPrefab = poofPrefab;

        newMember.speed = Random.Range(teamMin, teamMax);
        newMember.agility = Random.Range(teamMin, teamMax);
        newMember.range = Random.Range(teamMin, teamMax);
        newMember.wrapUp = Random.Range(teamMin, teamMax);
        newMember.coverage = Random.Range(teamMin, teamMax);


        return newMember;
    }
}
