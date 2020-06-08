using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lab_Astroids
{
    public class Asteroids
    {
        readonly GraphicsPath asteroids = new GraphicsPath();   //path of the asteroid
        readonly static Random ran = new Random();  //used to give the asteroid random speed, and shape
        readonly float xSpeed;  //horizontal speed of asteroid
        readonly float ySpeed;  //vertical speed of asteroid

        const int rotationSpeed = 1;    //rotating speed of the asteroid
        const int movingSpeed = 2;      //velocity of the asteroid
        const double fadeInValue = 40.0;//value used to determine when the asteroid has finished fading in
        int fadeIn;         //current value to see how much of the asteroid has faded in
        int Rotate;         //current rotational angle of the asteroid
        Rectangle window;   //size of the form
        public int AstSize { get; set; }    //gets current size of the asteroid (big, medium, small)
        public bool Dead { get; set; }      //determines if the asteroid has collided with a bullet or a ship
        public PointF Location { get; set; }//current location of the asteroid
        public Color Colour { get; set; }   //current colour of the asteroid

        /// <summary>
        /// initializes all the fields, as well as creating an asteroid
        /// </summary>
        /// <param name="position">initial location of the bullet (tip of the ship)</param>
        /// <param name="radius">size of the asteroid</param>
        /// <param name="size">form size</param>
        public Asteroids(PointF position, int radius, Rectangle size)
        {
            Location = position;
            window = size;
            Rotate = 0;
            xSpeed = (float)(ran.NextDouble() * movingSpeed - movingSpeed / 2);
            ySpeed = (float)(ran.NextDouble() * movingSpeed - movingSpeed / 2);
            asteroids = MakeAsteroid(radius, ran.Next(6, 13));
            Colour = Color.FromArgb(0, Color.Green);
            fadeIn = 0;
            AstSize = 1;
        }

        /// <summary>
        /// moves and rotates the asteroid, check how much it faded in, and wrap
        /// </summary>
        public void Tick()
        {
            //if asteroid is first being created, keep fading in as a green asteroid
            if (fadeIn < fadeInValue && AstSize == 1)
            {
                fadeIn++;

                //calculates the alpha value which will be used to see how much of the asteroid has faded in
                int alpha = (int)(fadeIn / fadeInValue * 255.0);
                Colour = Color.FromArgb(alpha, Color.Green);
            }
            //if the asteroid has fully faded in, turn it red
            else
                Colour = Color.Red;

            //rotate the asteroid
            Rotate += rotationSpeed;

            //move the asteroid's position according to its speed
            Location = new PointF(Location.X + xSpeed, Location.Y + ySpeed);

            //check boundaries and wrap
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
        /// get a copy of the transformed asteroid
        /// </summary>
        /// <returns></returns>
        public GraphicsPath GetPath()
        {
            //matrix to be applied to our shape
            Matrix transforms = new Matrix();

            transforms.Translate(Location.X, Location.Y);
            transforms.Rotate(Rotate);

            //clone the shape so the transform does not affect the original
            GraphicsPath cloned = (GraphicsPath)asteroids.Clone();

            //transform and rotate the path as specified above
            cloned.Transform(transforms);

            return cloned;
        }

        /// <summary>
        /// render the shape on the graphics surface with specified colour
        /// </summary>
        /// <param name="gr">graphics surface to render the shape on</param>
        public void Render(Graphics gr)
        {
            gr.DrawPath(new Pen(Colour), GetPath());
        }

        /// <summary>
        /// Method to create an asteroid
        /// </summary>
        /// <param name="radius">radius of the shape being drawn</param>
        /// <param name="vertCount">number of points exisiting within the shape (ex. triangle = 3)</param>
        /// <returns></returns>
        public GraphicsPath MakeAsteroid(int radius, int vertCount)
        {
            //array that holds the location of the 'points' of the asteroid
            PointF[] points = new PointF[vertCount];
            GraphicsPath gp = new GraphicsPath();

            //acquires the 'points' of the shape to be drawn on the canvas
            for (int i = 0; i < vertCount; i++)
            {
                double angle = (Math.PI * 2.0 / vertCount) * i;

                float xVal = (float)(Math.Cos(angle) * (radius - ran.NextDouble() * radius * 0.5));
                float yVal = (float)(Math.Sin(angle) * (radius - ran.NextDouble() * radius * 0.5));
                points[i] = new PointF(xVal, yVal);
            }

            //connect the acquired points by lines, which will be later be filled with colour
            gp.StartFigure();
            gp.AddLines(points);
            gp.CloseFigure();

            return gp;
        }
    }
}
