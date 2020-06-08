using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lab_Astroids
{
    public class Bullets
    {
        readonly GraphicsPath bullet = new GraphicsPath();  //path of the bullet
        public int maxDistTravel = 600;                     //max distance the bullet is able to travel
        const int rad = 1;      //bullet size of 1 pixel
        const int speed = 10;   //speed of the bullet
        readonly int rotate;    //angle of the bullet
        Rectangle window;       //size of the form
        PointF Location;        //current location fo the bullet
        Color colour;           //colour of the bullet
        public bool Dead { get; set; }          //indicate if the bullet has collided with an asteroid
        public int DisTravelled { get; set; }   //the distance the bullet has travelled

        /// <summary>
        /// initializes all the fields, as well as creating a bullet
        /// </summary>
        /// <param name="position">starting position of the bullet</param>
        /// <param name="rotation">angle of the bullet</param>
        /// <param name="rec">get the size of the form</param>
        public Bullets(PointF position, int rotation, Rectangle rec)
        {
            window = rec;
            Location = position;
            rotate = rotation;
            colour = Color.Yellow;  //set the colour of the bullet to yellow
            bullet = MakeBullets(); //create a bullet
        }

        /// <summary>
        /// get a copy of the transform bullet
        /// </summary>
        /// <returns>a copy of transformed bullet</returns>
        public GraphicsPath GetPath()
        {
            //matrix to be applied to the shape
            Matrix matrix = new Matrix();

            matrix.Translate(Location.X, Location.Y);

            //clone the shape so the transform does not affect the original
            GraphicsPath cloned = (GraphicsPath)bullet.Clone();

            //transform and widen the path to later check for intersection
            cloned.Widen(new Pen(Color.Yellow), matrix);

            return cloned;
        }

        /// <summary>
        /// render the shape on the graphics surface with specified colour
        /// </summary>
        /// <param name="gr">graphics surface to render the shape on</param>
        public void Render(Graphics gr)
        {
            gr.DrawPath(new Pen(colour), GetPath());
        }

        /// <summary>
        /// move the location of the bullet. check for boundaries, and wrap the bullets
        /// </summary>
        public void Tick()
        {
            DisTravelled += speed;  //get the total distance the bullet has travelled
            double angle = Math.PI * rotate / 180;  //get the angular direction of the bullet

            //when the bullet only has 100 more pixels to move before it gets removed, start fading the bullet
            if (maxDistTravel - DisTravelled < 100 && maxDistTravel - DisTravelled > 0)
            {
                int alpha = (int)((maxDistTravel - DisTravelled) / 100.0 * 255.0);
                colour = Color.FromArgb(alpha, Color.Yellow);
            }

            //had to invert the angles of the bullet due to how I made the ship
            Location = new PointF(Location.X + (float)(speed * -Math.Cos(angle)), Location.Y + (float)(speed * -Math.Sin(angle)));

            //check boundaries and wrap bullet
            if (Location.X > window.Width)
                Location = new PointF(0, Location.Y);
            if (Location.X < 0)
                Location = new PointF(window.Width, Location.Y);
            if (Location.Y > window.Height)
                Location = new PointF(Location.X, 0);
            if (Location.Y < 0)
                Location = new PointF(Location.X, window.Height);
        }

        /// <summary>
        /// creates an initial bullet
        /// </summary>
        /// <returns></returns>
        public GraphicsPath MakeBullets()
        {
            GraphicsPath gp = new GraphicsPath();

            //array that holds the location of the 'points' of the bullet 
            PointF[] points = new PointF[2];

            //points of the bullet is now assigned
            points[0] = new PointF(0, 0);
            points[1] = new PointF(rad, 0);

            //connect the acquired points by lines
            gp.StartFigure();
            gp.AddLines(points);
            gp.CloseFigure();

            return gp;
        }
    }
}
