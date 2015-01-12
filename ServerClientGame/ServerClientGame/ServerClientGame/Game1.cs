using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Networking;
using System.Linq;
using System;

namespace ServerClientGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public string Name { get { return "MyGame"; }  }

        internal static Game1 MakeGame(string[] args)
        {
            var input = "";
            if (!args.Contains("server") && !args.Contains("client"))
            {
                Console.Write("Server? ");

                input = Console.ReadLine();
            }

            if (!args.Contains("server") && input != "y") 
                return new Game1();

            var port = 3000;
            if (args.Length >= 2)
                port = Convert.ToInt32(args[1]);

#if DEBUG
            if (args.Length >= 3 && args[2].ToLower() == bool.TrueString.ToLower())
                Program.StartSecondProcess();
                
#endif

            return new Game1(port);
        }


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = Name + " Client";
            Console.Title = Window.Title;

            NetworkManager.Game = this;
            NetworkManager.Console = new CustomConsole(this);
            NetworkManager.Client = new Client(this);
            Components.Add(NetworkManager.Console);
            Components.Add(NetworkManager.Client);
    
        }

        public Game1(int port) : this()
        {
            NetworkManager.Server = new Server(this, port);
            Components.Add(NetworkManager.Server);

            Window.Title = Name + " Server";
            Console.Title = Window.Title;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            if ((NetworkManager.Server != null && !NetworkManager.Server.Alive) ||
                (NetworkManager.Client != null && !NetworkManager.Client.Alive))
                Exit();
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
