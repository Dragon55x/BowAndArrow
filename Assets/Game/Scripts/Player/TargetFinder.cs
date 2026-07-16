using UnityEngine;

namespace BAA
{
    public sealed class TargetFinder : MonoBehaviour
    {
        [SerializeField] private LayerMask targetMask;
        [SerializeField, Min(0.1f)] private float range = 10f;

        private readonly Collider[] _hits = new Collider[32];

        public CharacterHealth CurrentTarget { get; private set; }

        public float Range
        {
            get => range;
            set => range = Mathf.Max(0.1f, value);
        }

        public CharacterHealth FindNearest()
        {
            CurrentTarget = null;
            var count = Physics.OverlapSphereNonAlloc(transform.position, range, _hits, targetMask);
            var bestDistance = float.PositiveInfinity;

            for (var i = 0; i < count; i++)
            {
                var health = _hits[i].GetComponentInParent<CharacterHealth>();
                if (health == null || health.IsDead)
                {
                    continue;
                }

                var distance = (health.transform.position - transform.position).sqrMagnitude;
                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                CurrentTarget = health;
            }

            return CurrentTarget;
        }
    }
}
