using UnityEngine;

namespace BAA
{
    public sealed class ExperienceSettlementCounter
    {
        public int PendingCount { get; private set; }

        public void Register()
        {
            PendingCount++;
        }

        public void Unregister()
        {
            PendingCount = Mathf.Max(0, PendingCount - 1);
        }

        public void Reset()
        {
            PendingCount = 0;
        }
    }

    public sealed class ExperienceOrb : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float attractionDelay = 0.2f;
        [SerializeField, Min(0.1f)] private float moveSpeed = 9f;
        [SerializeField, Min(0.05f)] private float collectDistance = 0.3f;
        [SerializeField, Min(1)] private int experienceAmount = 10;

        private PlayerProgression _target;
        private float _remainingDelay;
        private bool _isPending;
        private static readonly ExperienceSettlementCounter SettlementCounter =
            new ExperienceSettlementCounter();

        public static int PendingCount => SettlementCounter.PendingCount;

        public void Initialize(int amount)
        {
            experienceAmount = Mathf.Max(1, amount);
        }

        private void OnEnable()
        {
            if (_isPending)
            {
                return;
            }

            _isPending = true;
            SettlementCounter.Register();
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

        private void OnDisable()
        {
            if (!_isPending)
            {
                return;
            }

            _isPending = false;
            SettlementCounter.Unregister();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            SettlementCounter.Reset();
        }
    }
}
