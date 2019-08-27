using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamfightTactics
{
    [CreateAssetMenu(fileName = "AttackUnitData", menuName = "TFT/AttackUnitData", order = 2)]
    public class AttackUnitData : TileUnitData
    {
        public float baseHealth;
        public float baseAttack;
        public float baseAttackRange;
        public float baseAttackRate;
    }
}
