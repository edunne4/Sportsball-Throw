using UnityEngine;

public abstract class Player : ScriptableObject {

    public new string name = "";
    public int number = 0;

    public int price = 0;

    public Sprite head;
    public Sprite body;
    public Sprite arm;
    public Sprite leg;

    public Color color = Color.white;




    public static readonly float speedDiv = 6.5f;
    public static readonly float agilityDiv = 16f;
    public static readonly float rangeDiv = 300f;
}
