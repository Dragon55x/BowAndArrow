using System.Collections.Generic;

namespace BAA
{
    public static class ShotPatternCalculator
    {
        public static float[] CalculateAngles(int forwardArrowCount, int diagonalArrowCount)
        {
            var result = new List<float>();
            var forward = forwardArrowCount <= 1
                ? new[] { 0f }
                : new[] { -4f, 4f };
            result.AddRange(forward);

            for (var i = diagonalArrowCount; i >= 1; i--)
            {
                result.Insert(0, -30f * i);
            }

            for (var i = 1; i <= diagonalArrowCount; i++)
            {
                result.Add(30f * i);
            }

            return result.ToArray();
        }
    }
}
