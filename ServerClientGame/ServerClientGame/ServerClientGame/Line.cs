using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNADrawing;

namespace ServerClientGame
{
    class Line : DrawableGameComponent 
    {
        static int MaxLength = 100;

        public Vector2 P1 { get; set; }

        private Vector2 _P2;

	    public Vector2 P2
	    {
		    get { return _P2;}
            set { _P2 = (Vector2.Normalize(value - P1) * MaxLength) + P1; }
	    }
	

        public float Length { get { return Vector2.Distance(P1, P2); } }

        public Line(Game game, Vector2 p1, Vector2 p2) : base(game)
        {
            P1 = p1;
            _P2 = p2;
        }

        internal void Draw(SpriteBatch spriteBatch, GameTime gameTime, Color color)
        {
            spriteBatch.DrawLineSegment(P1, P2, color, 1);
        }
    }
}
