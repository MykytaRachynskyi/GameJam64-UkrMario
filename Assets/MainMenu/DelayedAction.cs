using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WhatsInTheBox
{
    public class DelayedAction : MonoBehaviour
    {
        [SerializeField] float m_Delay;
        [SerializeField] UnityEvent m_OnAction;

        WaitForSeconds wfs = null;

        public void Execute()
        {
            if (wfs == null)
                wfs = new WaitForSeconds(m_Delay);

            StartCoroutine(DelayedActionRoutine(wfs));
        }

        IEnumerator DelayedActionRoutine(WaitForSeconds delay)
        {
            yield return delay;
            m_OnAction?.Invoke();
        }
    }
}