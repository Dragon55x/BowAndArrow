using UnityEngine;

namespace BAA
{
    public sealed class ExperienceOrb : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float attractionDelay = 0.2f;
        [SerializeField, Min(0.1f)] private float moveSpeed = 9f;
        [SerializeField, Min(0.05f)] private float collectDistance = 0.3f;
        [SerializeField, Min(1)] private int experienceAmount = 10;

        private PlayerProgression _target;
        private float _remainingDelay;

        public void Initialize(int amount)
        {
            experienceAmount = Mathf.Max(1, amount);
        }

        private void Start()
        {
            _remainingDelay = attractionDelay;
            var player = GameObject.FindGameObjectWithTag("Player");
            _target = player != null ? player.GetComponent<PlayerProgression>() : null;
            if (_target != null)
            {
                return;
            }

            Debug.LogError("ExperienceOrb requires an active Player with PlayerProgression.", this);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            transform.Rotate(0f, 180f * Time.deltaTime, 0f, Space.World);
            if (_remainingDelay > 0f)
            {
                _remainingDelay -= Time.deltaTime;
                return;
            }

            var targetPosition = _target.transform.position + Vector3.up * 0.8f;
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);
            if ((transform.position - targetPosition).sqrMagnitude > collectDistance * collectDistance)
            {
                return;
            }

            _target.AddExperience(experienceAmount);
            Destroy(gameObject);
        }
    }
}
