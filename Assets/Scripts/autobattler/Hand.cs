using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TeamfightTactics
{
    public class Hand : TileGroup
    {
        void OnEnable()
        {
            GameManager.Instance.RegisterHand(this);
        }

        void OnDisable()
        {
            GameManager.Instance.DeregisterHand(this);
        }
    }
}
