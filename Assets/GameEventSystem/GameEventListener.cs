using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WhatsInTheBox
{
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] GameEvent m_Event;
        [SerializeField] UnityEvent m_OnEvent;

        void Awake()
        {
            m_Event?.AddListener(Invoke);
        }

        void OnDestroy()
        {
            m_Event?.RemoveListener(Invoke);
        }

        public void Invoke()
        {
            m_OnEvent?.Invoke();
        }
    }
}