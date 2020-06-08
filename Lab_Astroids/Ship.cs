using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lab_Astroids
{
    public class Ship
    {
        public readonly GraphicsPath sp = new GraphicsPath();   //path of the ship
        public Rectangle window;//the size of the form
        public PointF location; //curent location of the ship
        public int rotate;      //current rotation of the ship

        //used to set or get colours of the shape
        public Color Colour { get; set; }

        //used to determine if the ship's has respawned or not
        public bool Revive { get; set; }

        /// <summary>
        /// initializes all the fields, as well as creating a ship
        /// </summary>
        /// <param name="position">starting position of the ship</param>
        /// <param name="size">the size of the form</param>
        public Ship(PointF position, Rectangle size)
        {
            window = size;
            location = position;
            rotate = 90;            //set the angle of the ship to be 90 so it faces up
            sp = MakeShip(10);   //create a ship
        }

        /// <summary>
        /// Check for boundaries, and wrap the ship
        /// </summary>
        public void Tick()
        {
            //check boundaries and wrap
            if (location.X > window.Width)
                location = new PointF(0, location.Y);
            if (location.X < 0)
                location = new PointF(window.Width, location.Y);
            if (location.Y > window.Height)
                location = new PointF(location.X, 0);
            if (location.Y < 0)
                location = new PointF(location.X, window.Height);
        }

        /// <summary>
        /// get a copy of the transformed ship
        /// </summary>
        /// <returns></returns>
        public GraphicsPath GetPath()
        {
            //matrix to be applied to the shape
            Matrix matrix = new Matrix();

            matrix.Translate(location.X, location.Y);
            matrix.Rotate(rotate);

            //clone the shape so the transform does not affect the original
            GraphicsPath cloned = (GraphicsPath)sp.Clone();

            //transform and rotate the path as specified above
            cloned.Transform(matrix);

            return cloned;
        }

        /// <summary>
        /// Colour the path for the drawn shape
        /// </summary>
        /// <param name="gr">graphics surface to render the shape on</param>
        public void Render(Graphics gr)
        {
            gr.DrawPath(new Pen(Colour), GetPath());
        }

        /// <summary>
        /// create an initial ship
        /// </summary>
        /// <param name="radius">size of the ship</param>
        /// <returns></returns>
        public GraphicsPath MakeShip(float radius)
        {
            //array that holds the location of the 'points' of the ship
            PointF[] points = new PointF[4];
            GraphicsPath gp = new GraphicsPath();

            //creates the points for the ship creation
            for (int i = 0; i < 4; i++)
            {
                //responsible for creating the 'legs' of the ship
                if (i == 0)
                    points[i] = new PointF(3, 0);

                //3 points to create a triangle
                else
                {
                    double angle = (Math.PI * 2.0 / 3) * i - 1;

                    float xVal = (float)(Math.Cos(angle) * radius);
                    float yVal = (float)(Math.Sin(angle) * radius);
                    points[i] = new PointF(xVal, yVal);
                }
            }

            //connect the acquired points by lines
            gp.StartFigure();
            gp.AddLines(points);
            gp.CloseFigure();

            return gp;
        }
    }
}
