using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    public string name;
    public CharacterType type;
    public List<Sprite> sprites = new List<Sprite>(); // 0: Idle, 1: Win, 2: Lose
    bool unlocked = false;
}