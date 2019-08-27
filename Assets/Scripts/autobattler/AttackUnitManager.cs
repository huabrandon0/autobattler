using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TeamfightTactics
{
    public class AttackUnitManager : SerializedMonoBehaviour
    {
        public static AttackUnitManager Instance { get; private set; }

        [SerializeField]
        public List<AttackUnit> AttackUnits { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            DontDestroyOnLoad(gameObject);

            AttackUnits = new List<AttackUnit>();
        }

        public void RegisterAttackUnit(AttackUnit attackUnit)
        {
            if (AttackUnits.Contains(attackUnit))
                return;

            AttackUnits.Add(attackUnit);
        }

        public void DeregisterAttackUnit(AttackUnit attackUnit)
        {
            if (!AttackUnits.Contains(attackUnit))
                return;

            AttackUnits.Remove(attackUnit);
        }
    }
}
