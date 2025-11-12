using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public int hp = 100;
    public int hpLossPerSecond = 0;
    public int damage = 100;
    public int pct_attackSize = 100;      // in % 
    public float attackSpeed = 1f;    // Attacks per second
    public int exp = 0;
    public int baseReflection = 0;
    public int armor = 0;
    public int bossArmor = 0;
    public int bossDamage = 0;
    public int pct_critRate = 0; // in %
    public int pct_critDamage = 100; // in %

    public int pct_additionalDamagePerEnemy = 10; // in %
    public int additionalRedBitsDrop = 5; 
    public int pct_spawnRate = 100; // in %



    [Header("Currencies")]
    public int redBits = 0;
    public int blueBits = 0;
    public int purpleBits = 0;

    public bool HasEnoughBits(string type, int amount)
    {
        return type switch
        {
            "redBits" => redBits >= amount,
            "blueBits" => blueBits >= amount,
            "purpleBits" => purpleBits >= amount,
            _ => false
        };
    }

    public void SpendBits(string type, int amount)
    {
        switch (type)
        {
            case "redBits": redBits -= amount; break;
            case "blueBits": blueBits -= amount; break;
            case "purpleBits": purpleBits -= amount; break;
        }
    }
}
