using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Networking;
using System.Linq;
using System;
using XNADrawing;
using System.Collections.Generic;

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

        #region Frames Per Second Field Region

        private float fps;
        private float updateInterval = 0.10f;
        private float timeSinceLastUpdate = 0.0f;
        private float frameCount = 0;

        #endregion
        
        
        Ball Ball;
        const int MaxLines = 10;
        List<Line> Lines = new List<Line>();
        Line tempLine;
        InputHandler InputHandler;



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
            InputHandler = new InputHandler(this);
            Components.Add(InputHandler);
 
            Ball = new Ball(this);
            Components.Add(Ball);

            this.IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
        }

        public Game1(int port) : this()
        {
            NetworkManager.Server = new Server(this, port);
            Components.Add(NetworkManager.Server);

            Window.Title = Name + " Server";
            this.IsMouseVisible = true;

            Console.Title = Window.Title;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        /// 
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            if (System.Windows.Forms.Screen.AllScreens.Count() > 1)
            {
                var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
                form.Location = new System.Drawing.Point(System.Windows.Forms.Screen.AllScreens[1].WorkingArea.Left, System.Windows.Forms.Screen.AllScreens[1].WorkingArea.Top);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            PrimitiveDrawer.LoadContent(GraphicsDevice);
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

            HandleInput();
            base.Update(gameTime);
        }

        private void HandleInput()
        {
 	        if (tempLine == null)
            {
                if (InputHandler.LeftMouseButtonDown())
                {
                    tempLine = new Line(this, new Vector2(InputHandler.MouseX(), InputHandler.MouseY()), new Vector2(InputHandler.MouseX(), InputHandler.MouseY()));
                }
            }
            else
            {
                Vector2 P2 = new Vector2(InputHandler.MouseX(), InputHandler.MouseY());
                if (tempLine.P1 != P2)
                {
                    tempLine.P2 = new Vector2(InputHandler.MouseX(), InputHandler.MouseY());
                    if (!InputHandler.LeftMouseButtonDown())
                    {
                        Lines.Add(new Line(this, tempLine.P1, tempLine.P2));
                        tempLine = null;
                        if (Lines.Count > MaxLines)
                        {
                            Lines.RemoveAt(0);
                        }
                    }
                }
                else
                    tempLine = null;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            spriteBatch.DrawLineSegment(new Vector2(Window.ClientBounds.Width / 2, 0),
                new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height), Color.White, 1);

            Ball.Draw(spriteBatch, gameTime);
            foreach (Line line in Lines)
            {
                line.Draw(spriteBatch, gameTime, Color.White);
            }
            if (tempLine != null)
                tempLine.Draw(spriteBatch, gameTime, Color.Green);

            spriteBatch.End();
            base.Draw(gameTime);

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCount++;
            timeSinceLastUpdate += elapsed;
            if (timeSinceLastUpdate > updateInterval)
            {
                fps = frameCount / timeSinceLastUpdate;
                this.Window.Title = "FPS: " + fps.ToString() + "Ball V (" + Ball.Velocity.X + ", " + Ball.Velocity.Y + ")";

                frameCount = 0;
                timeSinceLastUpdate -= updateInterval;
            }
        }
    }
}
