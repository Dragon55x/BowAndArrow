using System.Collections.Generic;
using UnityEngine;

namespace BAA
{
    public sealed class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile prefab;

        private readonly Queue<Projectile> _available = new Queue<Projectile>();
        private readonly HashSet<Projectile> _availableSet = new HashSet<Projectile>();

        public Projectile Get()
        {
            while (_available.Count > 0)
            {
                var projectile = _available.Dequeue();
                _availableSet.Remove(projectile);
                if (projectile == null)
                {
                    continue;
                }

                projectile.BindToPool(this);
                projectile.transform.SetParent(null, true);
                return projectile;
            }

            var instance = Instantiate(prefab, transform);
            instance.BindToPool(this);
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(null, true);
            return instance;
        }

        public void Release(Projectile projectile)
        {
            if (projectile == null || !_availableSet.Add(projectile))
            {
                return;
            }

            projectile.OnReleasedByPool();
            projectile.gameObject.SetActive(false);
            projectile.transform.SetParent(transform, false);
            _available.Enqueue(projectile);
        }
    }
}
