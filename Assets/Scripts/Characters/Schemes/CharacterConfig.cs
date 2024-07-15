using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Creates a preset to config individual characters
/// </summary>
[CreateAssetMenu(menuName = "Profiles/CharacterConfig")]
public class CharacterConfig : ScriptableObject
{
    [Header("Player Config")]
    public float fatalHeight = 4f;
    public float knockedTime = 3f;
    public Vector2 punchStrength = new Vector2(30, 70); 

    [Header("Player Animation Clips")]
    public int idleID;
    public int walkID;
    public int jogID;
    public int runID;

    [Header("Player SFX Clips")]
    public AudioClip jumpStart;
    public AudioClip jumpEnd;
    public AudioClip[] footSteps;
    public AudioClip[] punchSounds;

    [Header("Player Particles")]
    public GameObject coinBurst;
    public GameObject _bloodSplat;

}
