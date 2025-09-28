using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Samurai-Standoff/GameData")]
public class GameData : ScriptableObject
{
    public List<float> easy = new (){ 1f, 0.75f, 0.5f, 0.25f };
    public List<float> medium = new () { 0.75f, 0.5f, 0.3f };
    public List<float> hard = new (){ 0.5f, 0.4f, 0.3f, 0.2f, 0.1f };
}
