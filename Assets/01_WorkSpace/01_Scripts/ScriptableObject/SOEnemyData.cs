using UnityEngine;

[CreateAssetMenu(fileName = "SOEnemyData", menuName = "Game/Enemy Data")]
public class SOEnemyData : ScriptableObject
{
    public int hp = 100;
    public float multiplierBaseReflection = 1f;
    public int exp = 10;
}
