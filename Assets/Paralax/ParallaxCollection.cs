using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LPGD.Parallax
{
    public class ParallaxCollection : MonoBehaviour
    {
        [SerializeField] Transform m_TargetTransform;
        [Range(0f, 1f)]
        [SerializeField] float m_FollowPercent;
        [SerializeField] Vector3 m_Offset;

        Coroutine m_FollowingRoutine;

        void Awake()
        {
            StartFollowing();
        }

        void StartFollowing()
        {
            if (m_FollowingRoutine != null)
                StopCoroutine(m_FollowingRoutine);

            if (m_TargetTransform != null)
                m_FollowingRoutine = StartCoroutine(Follow());
        }

        IEnumerator Follow()
        {
            this.transform.position = Vector3.Lerp(this.transform.position, m_TargetTransform.position, m_FollowPercent);
            yield return null;
        }
    }
}