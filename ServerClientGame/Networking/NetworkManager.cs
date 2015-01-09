using Microsoft.Xna.Framework;

namespace Networking
{
    public static class NetworkManager
    {
        public static Server Server { get; set; }
        public static Client Client { get; set; }

        public static CustomConsole Console { get; set; }
        public static Game Game { get; set; }
    }
}
