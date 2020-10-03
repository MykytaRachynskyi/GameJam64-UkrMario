using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WhatsInTheBox
{
    [CreateAssetMenu(menuName = "Events/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        [SerializeField] List<Action> m_Listeners = new List<Action>();

        public void AddListener(Action listener)
        {
            if (!m_Listeners.Contains(listener))
                m_Listeners.Add(listener);
        }

        public void RemoveListener(Action listener)
        {
            if (m_Listeners.Contains(listener))
                m_Listeners.Remove(listener);
        }

        public void Invoke()
        {
            if (m_Listeners == null || m_Listeners.Count == 0)
                return;

            for (int i = 0; i < m_Listeners.Count; i++)
            {
                m_Listeners[i]?.Invoke();
            }
        }
    }
}
