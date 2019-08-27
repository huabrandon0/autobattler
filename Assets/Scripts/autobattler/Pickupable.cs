using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamfightTactics
{
    public class Selectable : MonoBehaviour { }

    [RequireComponent(typeof(Rigidbody))]
    public class Pickupable : Selectable, IPickupable
    {
        public Rigidbody Rigidbody { get; set; }

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }
    }

    public interface IPickupable
    {
        Rigidbody Rigidbody { get; set; }
    }
}
