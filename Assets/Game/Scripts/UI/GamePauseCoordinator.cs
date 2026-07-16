using System.Collections.Generic;
using UnityEngine;

namespace BAA
{
    public static class GamePauseCoordinator
    {
        private static readonly HashSet<object> Owners = new HashSet<object>();
        private static float _resumeTimeScale = 1f;

        public static void Acquire(object owner)
        {
            if (owner == null || !Owners.Add(owner))
            {
                return;
            }

            if (Owners.Count == 1)
            {
                _resumeTimeScale = Time.timeScale;
            }

            Time.timeScale = 0f;
        }

        public static void Release(object owner)
        {
            if (owner == null || !Owners.Remove(owner) || Owners.Count > 0)
            {
                return;
            }

            Time.timeScale = _resumeTimeScale;
        }

        public static void Reset(float timeScale = 1f)
        {
            Owners.Clear();
            _resumeTimeScale = Mathf.Max(0f, timeScale);
            Time.timeScale = _resumeTimeScale;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Owners.Clear();
            _resumeTimeScale = 1f;
        }
    }
}
