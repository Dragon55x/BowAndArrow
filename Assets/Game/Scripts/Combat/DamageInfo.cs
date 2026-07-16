namespace BAA
{
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, object source, bool isCritical = false)
        {
            Amount = amount;
            Source = source;
            IsCritical = isCritical;
        }

        public float Amount { get; }
        public object Source { get; }
        public bool IsCritical { get; }
    }
}
