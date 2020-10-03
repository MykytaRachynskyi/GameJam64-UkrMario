using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WhatsInTheBox
{
    public abstract class GameEventGeneric<T> : ScriptableObject
    {
        [SerializeField] protected List<Action<T>> m_Listeners = new List<Action<T>>();

        public void AddListener(Action<T> listener)
        {
            if (!m_Listeners.Contains(listener))
                m_Listeners.Add(listener);
        }

        public void RemoveListener(Action<T> listener)
        {
            if (m_Listeners.Contains(listener))
                m_Listeners.Remove(listener);
        }

        public void Invoke(T arg)
        {
            if (m_Listeners == null || m_Listeners.Count == 0)
                return;

            for (int i = 0; i < m_Listeners.Count; i++)
            {
                m_Listeners[i]?.Invoke(arg);
            }
        }
    }
}