using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tournament", menuName = "Tournament")]
public class Tournament : ScriptableObject
{
    public Team[] teams;

    public Sprite field;
}
