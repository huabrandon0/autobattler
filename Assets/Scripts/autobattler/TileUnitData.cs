using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamfightTactics
{
    [CreateAssetMenu(fileName = "TileUnitData", menuName = "TFT/TileUnitData", order = 1)]
    public class TileUnitData : ScriptableObject
    {
        public string unitName;
        public GameObject characterPrefab;
    }
}
