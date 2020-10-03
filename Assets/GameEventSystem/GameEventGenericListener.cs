using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WhatsInTheBox
{
    public class GameEventGenericListener<ArgumentType, GameEventType, UnityEventType> : MonoBehaviour
        where UnityEventType : UnityEvent<ArgumentType>
        where GameEventType : GameEventGeneric<ArgumentType>

    {
        [SerializeField] protected GameEventType m_Event;
        [SerializeField] protected UnityEventType m_UnityEvent;

        private void Awake()
        {
            m_Event?.AddListener(Invoke);
        }

        private void OnDestroy()
        {
            m_Event?.RemoveListener(Invoke);
        }

        public void Invoke(ArgumentType arg)
        {
            m_UnityEvent?.Invoke(arg);
        }
    }
}