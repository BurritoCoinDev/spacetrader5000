// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Math.c

using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class GameMath
    {
        // Preserves the original custom PRNG from Math.c exactly.
        private const uint DefaultSeedX = 521288629;
        private const uint DefaultSeedY = 362436069;

        private static uint _seedX = DefaultSeedX;
        private static uint _seedY = DefaultSeedY;

        public static void RandSeed(uint seed1, uint seed2)
        {
            _seedX = seed1 != 0 ? seed1 : DefaultSeedX;
            _seedY = seed2 != 0 ? seed2 : DefaultSeedY;
        }

        public static uint Rand()
        {
            const uint a = 18000;
            const uint b = 30903;

            _seedX = a * (_seedX & MaxWord) + (_seedX >> 16);
            _seedY = b * (_seedY & MaxWord) + (_seedY >> 16);

            return (_seedX << 16) + (_seedY & MaxWord);
        }

        public static int GetRandom(int maxVal) => (int)(Rand() % (uint)maxVal);

        // Integer square root matching the original Palm implementation.
        public static int Sqrt(int a)
        {
            int i = 0;
            while (i * i < a) ++i;
            if (i > 0 && (i * i - a) > (a - (i - 1) * (i - 1))) --i;
            return i;
        }

        public static long SqrDistance(SolarSystem a, SolarSystem b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public static long RealDistance(SolarSystem a, SolarSystem b)
            => Sqrt((int)SqrDistance(a, b));

        public static int Abs(int a)  => a < 0 ? -a : a;
        public static int Min(int a, int b) => a <= b ? a : b;
        public static int Max(int a, int b) => a >= b ? a : b;
        public static long Min(long a, long b) => a <= b ? a : b;
        public static long Max(long a, long b) => a >= b ? a : b;
    }
}
