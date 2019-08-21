using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    private static GameInfo instance;
    public static GameInfo _instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        instance = this;
    }
    public float SnakebodyLinght;
    public float snakeTouspeed;
    public float playSpeed;
}
