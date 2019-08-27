using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TeamfightTactics
{
    public class CharacterAnimationEventCalls : MonoBehaviour
    {
        public UnityEvent OnAttack { get; set; } = new UnityEvent();

        void InvokeOnAttack()
        {
            OnAttack.Invoke();
        }
    }
}
