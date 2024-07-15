using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable, Inspectable]
public class CharacterSet
{
    [Serialize, Inspectable]
    public GameObject bottom = null;
    [Serialize, Inspectable]
    public GameObject face = null;
    [Serialize, Inspectable]
    public GameObject head = null;
    [Serialize, Inspectable]
    public GameObject top = null;
    [Serialize, Inspectable]
    public int price = 0;
}