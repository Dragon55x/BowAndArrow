using NUnit.Framework;

namespace BAA.Tests
{
    public sealed class ShotPatternCalculatorTests
    {
        [TestCase(1, 0, new[] { 0f })]
        [TestCase(2, 0, new[] { -4f, 4f })]
        [TestCase(1, 1, new[] { -30f, 0f, 30f })]
        public void CalculateAngles_ReturnsSymmetricPattern(int forward, int diagonal, float[] expected)
        {
            Assert.That(ShotPatternCalculator.CalculateAngles(forward, diagonal), Is.EqualTo(expected));
        }
    }
}
