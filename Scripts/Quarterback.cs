using UnityEngine;

[CreateAssetMenu(fileName = "New Quarterback", menuName = "Player/Quarterback")]
public class Quarterback : Player {

    [Range(0, 100)]
    public int accuracy = 50;
    [Range(0, 100)]
    public int strength = 50;
    [Range(0, 100)]
    public int quickThinking = 50;

    //public bool isOwned = false;

    public Sprite ballArm;

    public static readonly float strengthMod = 5f;
    //private static readonly float accuracyDiv = 1f;
    public static readonly float QTDiv = 200f;
}
