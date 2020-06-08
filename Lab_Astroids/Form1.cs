using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Media;

namespace Lab_Astroids
{
    public partial class Form1 : Form
    {
        //struct that holds highscore details that will later be serialized to stored as a file
        [Serializable]
        struct Save
        {
            public string name; //the name the user has chosen, which will be displayed on the highscore board
            public int point;   //the score that will be displayed on the highscore board
        }

        //stopwatch
        readonly Stopwatch bulletSW = new Stopwatch();      //used to see how long it has been since the last bullet has been fired
        readonly Stopwatch shipRevive = new Stopwatch();    //used to make sure the ship's invincibility lasts for 1 second

        //lists
        readonly List<Asteroids> asteroids = new List<Asteroids>(); //used to keep track of current displayed asteroids
        readonly List<Bullets> bullets = new List<Bullets>();   //used to keep track of fired bullets
        readonly List<Ship> listLife = new List<Ship>();        //used to keep track of current number of lives
        List<Save> listScore = new List<Save>();                //used to keep track of current top 10 highscores

        readonly TextBox txtBox = new TextBox();    //textbox control which will allow the user to enter their name, which will
                                                    //be displayed on the highscore board
        readonly Random ran = new Random();         //used to spawn asteroids in random location
        int score;          //initial score
        int lives;          //user starts with 3 lives 
        int targetScore;    //grant one extra life to the user for every 10000 points they earn
        int startingAst;    //starting number of asteroids
        int level;          //the current level the user is on
        int index = -1;     //set it to an invalid index so it doesn't trigger unwanted events
        float speedX;       //current horizontal speeed of the ship
        float speedY;       //current vertical speed of the ship

        Keys key = Keys.None;   //used for getting keys used for menu control
        Ship ship = null;       //initialize the ship 
        Save dets;              //struct variable used to hold information which will be stored into a list

        //constant values
        const int bigAsteroid = 50;                 //size of the big asteroid
        const int medAsteroid = 20;                 //size of the medium asteroid
        const int smallAsteroid = 8;                //size of the small asteroid
        const int rotatingSpeed = 10;               //used to set how fast the ship would turn
        const float speedIncrement = (float)0.35;   //acceleration value of the ship
        const float maxSpeed = (float)7.0;          //max speed of the ship

        //boolean values
        bool _up = false;        //used to check the up key press
        bool _right = false;     //used to check the right key press
        bool _left = false;      //used to check the left key press
        bool _shoot = false;     //used to check the shoot key press
        bool _enter = false;     //used to check the enter key press
        bool gameOver = false;   //used to indicate the game is over and a new screen is displayed
        bool start = false;      //used to indicate the game has started and a new screen is displayed
        bool pause = false;      //used to indicate the game has puased and a new screen is displayed
        bool highScore = false;  //used to indicate the highscore screen is being displayed

        public Form1()
        {
            InitializeComponent();
            bulletSW.Restart();     //initialize timer for bullet
            shipRevive.Restart();   //initialize timer for ship revive
            txtBox.Location = new Point(ClientRectangle.Width / 2 - txtBox.Width / 2,   //place the textbox on the gameover screen 
                ClientRectangle.Height / 3 - txtBox.Height / 2);                        
            Controls.Add(txtBox);   //add the textbox to the control
            txtBox.Visible = false; //make it invisble to start with
            txtBox.MaxLength = 8;   //only allow to user to input 8 characters
        }

        /// <summary>
        /// ticks every 25ms, and used to tick and render all shapes and messages accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            Graphics gr = CreateGraphics();
            BinaryFormatter fileFormat = new BinaryFormatter();

            //used to center the messages both horizontally and vertically
            StringFormat sF = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            //create a new file if it doesn't exist already
            if (!File.Exists("save.txt"))
            {
                FileStream file = new FileStream("save.txt", FileMode.Create, FileAccess.Write);

                //fill the top 10 highscores with default values
                for (int i = 0; i < 10; i++)
                {
                    dets.name = "XXXXXXXXXX";   //fill the empty file with placement name value
                    dets.point = 0;             //fill the empty file with placement score value
                    listScore.Add(dets);        //add that values to the score list
                }
                //serialize, and store it into the created file
                fileFormat.Serialize(file, listScore);
                file.Close();
            }

            //file exists, open it and store it
            else
            {
                //open up the file and store the information into a list
                FileStream file = new FileStream("save.txt", FileMode.Open, FileAccess.Read);
                listScore = (List<Save>)fileFormat.Deserialize(file);
                
                file.Close();
            }

            using (BufferedGraphicsContext bgc = new BufferedGraphicsContext())
            {
                using (BufferedGraphics bg = bgc.Allocate(gr, ClientRectangle))
                {
                    //instructions page
                    if (!start && !gameOver && !highScore)
                    {
                        bg.Graphics.Clear(Color.Black);

                        //give instructions to the user when they first open the game, or when they restart the game
                        bg.Graphics.DrawString("Welcome\nTo Shoot, Press 'X'\nTo Start the game, Press 'F'\n'P' to Pause/Resume\n'T' to look at highscores",
                            new Font("Times New Roman", 30), new SolidBrush(Color.Yellow),
                            ClientRectangle.Width / 2, ClientRectangle.Height / 2, sF);
                        bg.Render();

                        //start the game
                        if (key == Keys.F)
                        {
                            start = true;
                            key = Keys.None;

                            //start the game with default values and clear screen
                            bulletSW.Restart();     //restart timer for bullets
                            shipRevive.Restart();   //restart timer for ship
                            bullets.Clear();        //remove all bullets from the previous game
                            asteroids.Clear();      //remove all asteroids from the previous game
                            score = 0;              //reset the current score
                            targetScore = 10000;    //reset the target score, which will be used to give the user additional life
                                                    //for every score increment of 10,000
                            lives = 2;              //user starts off with 3 lives(2 will be displayed on the top left corner)
                            ship = null;            //nullify the ship
                            speedX = 0;             //reset the horizontal speed of ship
                            speedY = 0;             //reset the vertical speed of ship
                            startingAst = 5;        //starting number of asteroids will be reset to 5
                            level = 1;              //starting level will be 1
                            index = -1;             //index used for highsore keeping will be set to an invalid index
                            bg.Render();            
                        }

                        //view highscore
                        if (key == Keys.T)
                        {
                            highScore = true;   
                            key = Keys.None;    
                        }
                    }

                    //highscore screen
                    else if (highScore)
                    {
                        //go back to the instructions page
                        if (key == Keys.T)
                        {
                            highScore = false;
                            key = Keys.None;
                        }
                        bg.Graphics.Clear(Color.Black);

                        //show highscore
                        bg.Graphics.DrawString("HighScore\n" +
                            string.Format("{0, -15} {1, 15}", " Name", "Score") + "\n",
                            new Font("Times New Roman", 25), new SolidBrush(Color.Yellow),
                            ClientRectangle.Width / 2, ClientRectangle.Height / 12, sF);

                        //displays the name
                        bg.Graphics.DrawString(
                            $" 1.{listScore[0].name,-10}\n" +
                            $" 2.{listScore[1].name,-10}\n" +
                            $" 3.{listScore[2].name,-10}\n" +
                            $" 4.{listScore[3].name,-10}\n" +
                            $" 5.{listScore[4].name,-10}\n" +
                            $" 6.{listScore[5].name,-10}\n" +
                            $" 7.{listScore[6].name,-10}\n" +
                            $" 8.{listScore[7].name,-10}\n" +
                            $" 9.{listScore[8].name,-10}\n" +
                            $"10.{listScore[9].name,-10}",
                            new Font("Consolas", 20), new SolidBrush(Color.Yellow),
                            ClientRectangle.Width / 3, ClientRectangle.Height / 2, sF);

                        //displays the socre
                        bg.Graphics.DrawString(
                            $"{listScore[0].point:D9}\n" +
                            $"{listScore[1].point:D9}\n" +
                            $"{listScore[2].point:D9}\n" +
                            $"{listScore[3].point:D9}\n" +
                            $"{listScore[4].point:D9}\n" +
                            $"{listScore[5].point:D9}\n" +
                            $"{listScore[6].point:D9}\n" +
                            $"{listScore[7].point:D9}\n" +
                            $"{listScore[8].point:D9}\n" +
                            $"{listScore[9].point:D9}\n",
                            new Font("Consolas", 20), new SolidBrush(Color.Yellow),
                            ClientRectangle.Width - ClientRectangle.Width / 3, ClientRectangle.Height / 2, sF);

                        bg.Render();
                    }

                    //gameplay screen
                    else
                    {
                        //pause game
                        if (key == Keys.P)
                        {
                            pause = !pause;
                            key = Keys.None;
                        }

                        //not paused
                        if (!pause)
                        {
                            //game start
                            if (!gameOver && start)
                            {
                                bg.Graphics.Clear(Color.Black);

                                //initialize ship
                                if (ship == null)
                                {
                                    ship = new Ship(new PointF(ClientRectangle.Width / 2, ClientRectangle.Height / 2), ClientRectangle)
                                    {
                                        Colour = Color.Cyan
                                    };

                                    //display the ship lives as shape of ships on the top left corner of the screen
                                    for (int i = 0; i < lives; i++)
                                    {
                                        Ship life = new Ship(new PointF(30 + i * 20, 60), ClientRectangle)
                                        {
                                            Colour = Color.Cyan
                                        };
                                        listLife.Add(life);
                                    }

                                    //when the game starts/restarts, draw the starting asteroids
                                    for (int i = 0; i < startingAst; i++)
                                        asteroids.Add(new Asteroids(new PointF(ran.Next(ClientRectangle.Width), ran.Next(ClientRectangle.Height)), bigAsteroid, ClientRectangle));
                                }

                                //render the lives
                                listLife.ForEach(x => x.Render(bg.Graphics));

                                //move bullets and asteroids
                                ship.Tick();
                                asteroids.ForEach(x => x.Tick());
                                bullets.ForEach(x => x.Tick());

                                //display the score and lives on the top left corner
                                bg.Graphics.DrawString($"{score:D6}", new Font("Times New Roman", 20),
                                    new SolidBrush(Color.Gray), 15, 15);
                                bg.Graphics.DrawString($"Level:{level}", new Font("Times New Roman", 20), new SolidBrush(Color.Gray),
                                    ClientRectangle.Width / 2, 15, sF);

                                //rotate the ship to the left
                                if (_left)
                                {
                                    ship.rotate -= rotatingSpeed;

                                    //keep the ship rotation between 0 and 360
                                    if (ship.rotate < 0)
                                        ship.rotate = 360 + ship.rotate;
                                }

                                //rotate the ship to the right
                                if (_right)
                                {
                                    ship.rotate += rotatingSpeed;

                                    //keep the ship rotation between 0 and 360
                                    if (ship.rotate > 360)
                                        ship.rotate %= 360;
                                }

                                //accelerate
                                if (_up)
                                {
                                    //get ship's speed value
                                    speedX += (float)Math.Sin(Math.PI * (ship.rotate - 90) / 180) * speedIncrement;
                                    speedY += (float)Math.Cos(Math.PI * (ship.rotate - 90) / 180) * -speedIncrement;

                                    //the speed will become either positive or negative. Limit the max speed of the ship
                                    if (speedX > maxSpeed)
                                        speedX = maxSpeed;
                                    if (speedX < -maxSpeed)
                                        speedX = -maxSpeed;
                                    if (speedY > maxSpeed)
                                        speedY = maxSpeed;
                                    if (speedY < -maxSpeed)
                                        speedY = -maxSpeed;

                                    //move the ship's location
                                    ship.location.X += speedX;
                                    ship.location.Y += speedY;

                                    //draw thrusters
                                    GraphicsPath gp = new GraphicsPath();
                                    PointF[] thrust = new PointF[3];

                                    //creates a triangle shape
                                    thrust[0] = new PointF(5, 0 );
                                    thrust[1] = new PointF(0, 15);
                                    thrust[2] = new PointF(-5, 0);
                                    
                                    //adds the pin points of the triangle to the figure
                                    gp.StartFigure();
                                    gp.AddLines(thrust);
                                    gp.CloseFigure();

                                    //move the triangle to the end of the ship, and rotate it accordingly
                                    Matrix matrix = new Matrix();
                                    matrix.Translate(ship.GetPath().PathPoints[0].X, ship.GetPath().PathPoints[0].Y);
                                    matrix.Rotate(ship.rotate - 90);
                                    gp.Transform(matrix);
                                    bg.Graphics.DrawPath(new Pen(Color.Red), gp);
                                }

                                //momentum loss of the ship when the 'up' key is not pressed
                                else
                                {
                                    //limit the speed so it goes to 0 in the end. Both negative and positive value will move the ship
                                    if (speedX > 0)
                                    {
                                        speedX -= speedIncrement / 2;
                                        if (speedX < 0)
                                            speedX = 0;
                                    }
                                    if (speedX < 0)
                                    {
                                        speedX += speedIncrement / 2;
                                        if (speedX > 0)
                                            speedX = 0;
                                    }
                                    if (speedY > 0)
                                    {
                                        speedY -= speedIncrement / 2;
                                        if (speedY < 0)
                                            speedY = 0;
                                    }
                                    if (speedY < 0)
                                    {
                                        speedY += speedIncrement / 2;
                                        if (speedY > 0)
                                            speedY = 0;
                                    }

                                    //move the ship
                                    ship.location.X += speedX;
                                    ship.location.Y += speedY;
                                }

                                //shoot a bullet if the current bullets displayed is less than 8, and if 200ms has elapsed since the last bullet has been shot
                                if (_shoot && bullets.Count < 8 && bulletSW.ElapsedMilliseconds >= 200)
                                {
                                    //play a laser sound to indicate shooting
                                    SoundPlayer sound = new SoundPlayer(Properties.Resources.laser3);
                                    sound.Play();

                                    //add the bullet to a list
                                    bullets.Add(new Bullets(new PointF(ship.GetPath().PathPoints[2].X, ship.GetPath().PathPoints[2].Y), ship.rotate, ClientRectangle));

                                    //restart the timer for bullet
                                    bulletSW.Restart();
                                }

                                //make the ship invincible for one second after it respawns
                                if (shipRevive.ElapsedMilliseconds >= 1000)
                                {
                                    ship.Colour = Color.Cyan;
                                    ship.Revive = false;
                                    shipRevive.Stop();
                                }

                                //the level is completed, indicate that the user is now on the next level. Increase the number of asteroids being created
                                if (asteroids.Count == 0)
                                {
                                    level++;
                                    startingAst += 2;
                                    bg.Graphics.Clear(Color.Black);

                                    //display a message indicating the next level is being loaded
                                    bg.Graphics.DrawString($"Level Completed\nNext Round: {level}", new Font("Times New Roman", 30), new SolidBrush(Color.Gray),
                                        ClientRectangle.Width / 2, ClientRectangle.Height / 2, sF);
                                    for (int i = 0; i < startingAst; i++)
                                        asteroids.Add(new Asteroids(new PointF(ran.Next(ClientRectangle.Width), ran.Next(ClientRectangle.Height)), bigAsteroid, ClientRectangle));
                                    bg.Render();

                                    //display for 1 second
                                    Thread.Sleep(1000);
                                }

                                //check if any bullets encountered asteroids
                                foreach (Bullets b in bullets)
                                {
                                    foreach (Asteroids a in asteroids)
                                    {
                                        Region ast = new Region(a.GetPath());
                                        Region inter = new Region(b.GetPath());

                                        inter.Intersect(ast);

                                        //check if the bullet and an asteroid intersected
                                        if (!inter.IsEmpty(bg.Graphics))
                                        {
                                            //award the user with appropriate score
                                            //big asteroid = 100 points
                                            if (a.AstSize == 1)
                                                score += 100;
                                            //medium asteroid = 200 points
                                            if (a.AstSize == 2)
                                                score += 200;
                                            //small asteroid = 300 points
                                            if (a.AstSize == 3)
                                                score += 300;

                                            //up the variable value of the asteroid to be used later to determine the asteroid's size
                                            a.AstSize++;

                                            //mark the asteroid and bullet as collided
                                            a.Dead = true;
                                            b.Dead = true;
                                            break;
                                        }
                                    }

                                    //stop checking for more intersections
                                    if (b.Dead)
                                        break;
                                }

                                //if an asteroid gets hit by a bullet, check the size of the asteroid and then determine if it needs
                                //to be broken into smaller pieces
                                if (asteroids.Any(x => x.Dead))
                                {
                                    for (int i = 0; i < asteroids.Count; i++)
                                    {
                                        //break into smaller astroids
                                        if (asteroids[i].Dead && asteroids[i].AstSize < 4)
                                        {
                                            for (int j = 0; j < asteroids[i].AstSize; j++)
                                            {
                                                //break into two medium pieces
                                                if (asteroids[i].AstSize == 2)
                                                {
                                                    asteroids.Add(new Asteroids(asteroids[i].Location, medAsteroid, ClientRectangle));
                                                    asteroids[asteroids.Count - 1].AstSize = asteroids[i].AstSize;
                                                }

                                                //break into three small pieces
                                                else if (asteroids[i].AstSize == 3)
                                                {
                                                    asteroids.Add(new Asteroids(asteroids[i].Location, smallAsteroid, ClientRectangle));
                                                    asteroids[asteroids.Count - 1].AstSize = asteroids[i].AstSize;
                                                }

                                                //smallest asteroid has been destroyed. Remove from the list
                                                else                                                    
                                                    asteroids.Remove(asteroids[i]);
                                            }
                                            break;
                                        }
                                    }
                                }

                                //remove anything that has been marked as dead
                                bullets.RemoveAll(x => x.Dead);
                                bullets.RemoveAll(x => x.DisTravelled > x.maxDistTravel);
                                asteroids.RemoveAll(x => x.Dead);

                                //user gains another life with every increment of 10000
                                if (score >= targetScore)
                                {
                                    lives++;
                                    targetScore += 10000;

                                    //draw a shape of a ship on the top left corner of the screen, which indicates the number of lives the user currently has
                                    Ship life = new Ship(new PointF(30 + (listLife.Count) * 20, 60), ClientRectangle)
                                    {
                                        Colour = Color.Cyan
                                    };
                                    listLife.Add(life);
                                }

                                //check if any asteroids hit the ship
                                foreach (Asteroids a in asteroids)
                                {
                                    //check if the asteroid have been fully rendered
                                    if (a.Colour == Color.Red && !ship.Revive)
                                    {
                                        Region ast = new Region(a.GetPath());
                                        Region inter = new Region(ship.GetPath());

                                        //check if the ship and an asteroid has collided
                                        inter.Intersect(ast);
                                        if (!inter.IsEmpty(bg.Graphics))
                                        {
                                            //ship is out of lives. GameOver
                                            if (lives == 0)
                                            {
                                                gameOver = true;
                                                break;
                                            }

                                            //ship has more lives to spare
                                            else
                                            {
                                                lives--;
                                                listLife.RemoveAt(listLife.Count-1);    //remove one of the life indicator on the top left corner
                                                a.AstSize++;    //indicate the asteroid's getting broken into smaller pieces
                                                a.Dead = true;  //indicate asteroid's collision

                                                //create new ship with faded colour, indicating that the ship's been revived
                                                ship = new Ship(new PointF(ClientRectangle.Width / 2, ClientRectangle.Height / 2), ClientRectangle)
                                                {
                                                    Colour = Color.FromArgb(128, Color.Cyan)
                                                };
                                                ship.Revive = true;
                                                shipRevive.Restart();   //restart timer for the new ship
                                                speedX = 0; //reset ship's horizontal speed
                                                speedY = 0; //reset ship's vertical speed

                                                break;
                                            }
                                        }
                                    }
                                }

                                //render all shapes
                                ship.Render(bg.Graphics);
                                bullets.ForEach(x => x.Render(bg.Graphics));
                                asteroids.ForEach(x => x.Render(bg.Graphics));

                                bg.Render();
                            }


                            //game over
                            if (gameOver)
                            {
                                bg.Graphics.Clear(Color.Black);

                                //find out if the score that the user got is high enough to be included in the top 10 highscores. If yes,
                                //find out where the score should be placed within the highscore board
                                for (int i = 0; i < listScore.Count; i++)
                                {
                                    if (score > listScore[i].point)
                                    {
                                        index = i;
                                        break;
                                    }
                                }

                                //if the score is able to be added within the top 10 list, allow the user to choose their name and add to the list
                                if (index != -1)
                                {
                                    txtBox.Focus();     //give the textbox focus for the user

                                    //make the textbox visible to the user
                                    if (!txtBox.Visible)
                                        txtBox.Visible = true;

                                    //display that the game is over, and that the user can add their name and score to the board
                                    bg.Graphics.DrawString("You Lost... Try Again?",
                                        new Font("Times New Roman", 30),
                                        new SolidBrush(Color.Yellow), ClientRectangle.Width / 2, ClientRectangle.Height / 2, sF);
                                    bg.Graphics.DrawString("Enter your name to be on the wall of HighScore",
                                        new Font("Times New Roman", 30),
                                        new SolidBrush(Color.Yellow), ClientRectangle.Width / 2, ClientRectangle.Height / 4, sF);
                                    
                                    dets.point = score; //give struct variable the current score
                                    txtBox.KeyDown += TxtBox_KeyDown;   //create event handler for the textbox keydown
                                    txtBox.KeyUp += TxtBox_KeyUp;       //create event handler for the textbox keyup

                                    //the enter button has been detected. Add the details to the board, remove the score that has now been pushed to the 11th place
                                    if (_enter)
                                    {
                                        dets.name = Controls[0].Text;   //get the text that the user has typed in for the highscore
                                        listScore.Insert(index, dets);  //place the score in the right place
                                        listScore.RemoveAt(listScore.Count - 1);    //remove the score that's been pushed to be 11th place
                                        gameOver = false;   //reset the gameover flag
                                        start = false;      //reset the start flag
                                        pause = false;      //reset the pause flag

                                        //save the score to the save file
                                        FileStream file = new FileStream("save.txt", FileMode.Open, FileAccess.Write);
                                        fileFormat.Serialize(file, listScore);
                                        file.Close();

                                        //make the textbox invisible, clear the textbox, and remove focus from it
                                        txtBox.Visible = false;
                                        txtBox.Clear();
                                        Focus();
                                    }
                                }

                                //no new highscore. Display a generic message
                                else
                                {
                                    bg.Graphics.DrawString("You Lost... Try Again?\nPress 'F' to Restart",
                                        new Font("Times New Roman", 30),
                                        new SolidBrush(Color.Yellow), ClientRectangle.Width / 2, ClientRectangle.Height / 2, sF);

                                    //allow the user to go back to the instructions page
                                    if (key == Keys.F)
                                    {
                                        gameOver = false;
                                        start = false;
                                        pause = false;
                                        key = Keys.None;
                                    }
                                }

                                bg.Render();
                            }
                        }

                        //game paused. display
                        else
                        {
                            //render all shapes to still show the user what was happenning at the time
                            ship.Render(bg.Graphics);
                            bullets.ForEach(x => x.Render(bg.Graphics));
                            asteroids.ForEach(x => x.Render(bg.Graphics));

                            //display lives 
                            listLife.ForEach(x => x.Render(bg.Graphics));

                            //display the score and lives on the top left corner
                            bg.Graphics.DrawString($"{score:D6}", new Font("Times New Roman", 20),
                                new SolidBrush(Color.Gray), 15, 15);
                            bg.Graphics.DrawString($"Level:{level}", new Font("Times New Roman", 20), new SolidBrush(Color.Gray),
                                ClientRectangle.Width / 2, 15, sF);

                            //give the user an option to restart the game
                            bg.Graphics.DrawString("Paused\n\nPress 'Q' To Restart The Game",
                                new Font("Times New Roman", 30),
                                new SolidBrush(Color.Yellow), ClientRectangle.Width / 2, ClientRectangle.Height / 2, sF);
                            bg.Render();

                            //restart game
                            if (key == Keys.Q)
                            {
                                gameOver = false;
                                start = false;
                                pause = false;
                                key = Keys.None;
                            }
                            //wanted to have the 'paused' text blink... might have to use threading here
                            //Thread.Sleep(500);

                            //bg.Graphics.Clear(Color.Black);

                            //ship.Render(bg.Graphics);
                            //bullets.ForEach(x => x.Render(bg.Graphics));
                            //asteroids.ForEach(x => x.Render(bg.Graphics));

                            //bg.Render();
                            //Thread.Sleep(500);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// check if the enter key has been depressed while the focus was on the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                _enter = false;
        }

        /// <summary>
        /// check if the enter key has been pressed while the focus was on the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                _enter = true;
        }

        /// <summary>
        /// check for button presses while the form is in focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //used to check page moving keys
            key = e.KeyCode;

            //keep the buttons that were pressed and being pressed as activated
            if (e.KeyCode == Keys.Up)
                _up = true;
            if (e.KeyCode == Keys.Left)
                _left = true;
            if (e.KeyCode == Keys.Right)
                _right = true;
            if (e.KeyCode == Keys.X)
                _shoot = true;
        }

        /// <summary>
        /// check for button depresses while the form is in focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //used to check page moving keys
            key = Keys.None;

            //get rid of the buttons that were depressed
            if (e.KeyCode == Keys.Up)
                _up = false;
            if (e.KeyCode == Keys.Left)
                _left = false;
            if (e.KeyCode == Keys.Right)
                _right = false;
            if (e.KeyCode == Keys.X)
                _shoot = false;
        }
    }
}
