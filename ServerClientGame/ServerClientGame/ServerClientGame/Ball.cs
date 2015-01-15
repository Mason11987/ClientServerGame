
namespace ServerClientGame
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using XNADrawing;
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Ball : DrawableGameComponent
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public static Vector2 Gravity = new Vector2(0, 50f);
        public float Radius { get; set; }

        public Ball(Game game) : base(game)
        {
            Position = new Vector2(5, 200);
            Velocity = new Vector2(80, -1);
            Radius = 5;
        }


        public override void Update(GameTime gameTime)
        {
            Position += Vector2.Multiply(Velocity, (float)(gameTime.ElapsedGameTime.TotalSeconds));
            Velocity += Vector2.Multiply(Gravity , (float)(gameTime.ElapsedGameTime.TotalSeconds)); ;
            base.Update(gameTime);
            if (Position.X + Radius >= Game.Window.ClientBounds.Width)
                Velocity.X = -Velocity.X;
            if (Position.X - Radius <= 0)
                Velocity.X = -Velocity.X;
            if (Position.Y + Radius >= Game.Window.ClientBounds.Height)
                Velocity.Y = -Velocity.Y;
            if (Position.Y - Radius <= 0)
                Velocity.Y = -Velocity.Y;


        }

        internal void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.DrawCircle(Position, Radius, Color.Red, 1);
        }
    }
}
