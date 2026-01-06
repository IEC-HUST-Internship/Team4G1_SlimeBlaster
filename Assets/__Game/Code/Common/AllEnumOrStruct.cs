
public enum EnumStat
{
    hp,
    hpLossPerSecond,
    damage,
    attackSizeCount,
    attackSpeed,
    exp,
    level,
    baseReflection,
    armor,
    bossArmor,
    bossDamage,
    critRatePercent,
    critDamagePercent,
    additionalDamagePerEnemyInAreaPercent,
    additionalBlueBitsDropPerEnemy,
    additionalPinkBitsDropPerEnemy,
    additionalYellowBitsDropPerEnemy,
    additionalGreenBitsDropPerEnemy,
    spawnRatePercent,
    baseHp,
    baseDamage,
    baseArmor,
    addHealthPerEnemyHit,  // 💚 Heal HP per enemy hit
    addHealthPerEnemyKill,  // 💚 Heal HP per enemy killed
    currencyPickupRadiusIncreasePercent,  // 🧲 Increase pickup radius by %
    additionalAttackSpeedIncreasePercent  // ⚔️ Attack speed bonus % (50 = 1.5x, 120 = 2.2x)
}

public enum EnumCurrency
{
    blueBits,
    pinkBits,
    yellowBits,
    greenBits,
    xpBits  // 🌟 Earned on level up (+1 per level)
}