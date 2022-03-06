namespace Spellcard
{
    public static class LaresUtils
    {
        public static class Random
        {
            private static System.Random rand = new();
            public static int Int(int min, int max) => rand.Next(min, max + 1);
        }
    }
}
