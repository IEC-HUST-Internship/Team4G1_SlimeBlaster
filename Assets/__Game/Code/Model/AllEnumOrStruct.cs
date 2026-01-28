
public enum EnumStat
{
    hp,
    hpLossPerSecond,
    damage,
    attackSizeCount,
    secondPerAttack,  // ⏱️ Base seconds between attacks (2 = attack every 2 sec)
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
    additionalAttackSpeedIncreasePercent  // ⚔️ Reduces secondPerAttack by % (50 = 50% faster)
}

public enum EnumCurrency
{
    blueBits,
    pinkBits,
    yellowBits,
    greenBits,
    xpBits  // 🌟 Earned on level up (+1 per level)
}