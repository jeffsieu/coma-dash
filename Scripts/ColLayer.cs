public class ColLayer
{
    public class Bit
    {
        public static int Environment = 0;
        public static int Player = 1;
        public static int Enemies = 2;
        public static int Projectiles = 3;
    }
    public static uint Environment = 1u << Bit.Environment;
    public static uint Player = 1u << Bit.Player;
    public static uint Enemies = 1u << Bit.Enemies;
    public static uint Projectiles = 1u << Bit.Projectiles;

}
