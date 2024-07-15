using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerStats
{
    public static int coins = 0;
    public static int lives = 3;
    public static float health = 100;
}
public enum Status
{
    NORMAL,
    BUSY,
    DEAD,
    PATROL,
    CHASING,
    ATTACKING,
    PERSISTING,
    BLOCKING,
    FALLEN,
    RESETINGBONES,
    STANDINGUP
}

public enum AttackType
{
    PUNCH
}