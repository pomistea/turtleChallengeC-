/*
 * Tite:    Turtle Challenge
 * Author:  Paul Mitchell
 * Date:  29th of January 2019
 * 
 * Description: Game to try to get Turtle safely across a minefield.
 *              Game settings, board layout etc are uploaded from files/settings.json
 *              The Turtle's moves are uploaded from files/moves.json
 *              
 * Technical: The game consists of 2 objects, the Turtle and MineBoard.
 *            Fields and functionality are encapsulated within these class. 
 *            Accessing functionality is straighforward e.g.Turtle.Move(); Turtle.Rotate();
 *            MindBoard.LoadSettings(); MineBoard.LoadMoves(); Mineboard.CheckStatus(Turtle);
 *            
 *            The static FileUtils class contains a generic json file reader, to avoid duplication of code for reading in the 2 files.
 *            Note: Using Moves.moveString is a bit clunky but I left it like that so as to be able to reuse the json reader methodin FileUtils.
 *            
 *            Basic validation is carried out. Methods return false in error conditions. 
 *            A try catch is used for the file read but nothing is done with caught exceptioms.
 *            
 *            On execution of Main(), a number of tests run first, followed by the game itself.
 *            When the game is being played, each move is displayed along with then Turtle's position and direction after the move has been affected.          
 */           

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

using static Turtle_Challenge.Direction;
using static Turtle_Challenge.Status;


namespace Turtle_Challenge
{
    class Program
    {
        // Entry point for Console App.
        static void Main(string[] args)
        {
            // Perform a number of tests on the game.
            TestGame();
           
            // Read the settings and moves from files and play the game.
            PlayGame();
        }

        // Plays the game based on the contents of the settings and moves files.
        static void PlayGame()
        {
            MineBoard board = new MineBoard();

            if (!board.LoadSettings())
                return;         

            if (!board.LoadMoves())
                return;

            int turtleStartX = board.Settings.startPosition.Item1;
            int turtleStartY = board.Settings.startPosition.Item2;
            Direction startDirection;

            if(!Enum.TryParse(board.Settings.startDirection, out startDirection))
            {
                Console.WriteLine("Error parsing start direction!");
                return;
            }

            // Create the turtle.
            Turtle turtle = new Turtle(turtleStartX, turtleStartY, startDirection);
            //turtle.Direction = startDirection;

            Console.WriteLine("Start Game - Turtle Position: " + turtle.GetTurtleUpdate());

            // Loop through the moves.
            foreach (char move in board.Moves.moveString.ToLower())
            {
                switch(move)
                {
                    case 'm':
                    {
                         turtle.Move();
                         Console.WriteLine("Move");
                         break;
                    }
                    case 'r':
                    {
                        turtle.Rotate();
                        Console.WriteLine("Rotate");
                        break;
                    }
                 default:
                    {
                        Console.WriteLine("Error - Illegal move!");
                        return; 
                    }
                }

                Console.WriteLine("Turtle Position: " + turtle.GetTurtleUpdate() + System.Environment.NewLine);          

                // If NOT Still_In_Danger the game must be over.
                if (board.CheckStatus(turtle) != Still_In_Danger)
                    break;
            }

            Console.WriteLine("Game Over - Turtle's Status: " + board.CheckStatus(turtle).ToString());
        }

        // Execute a number of tests consisting of move strings and expected results.
        static void TestGame()
        {
            /*
             * BOARD LAYOUT (T (Turtle) facing North)
             * 
             * 00000
             * TX000
             * 00XXE
             * 00000
             */

            Settings testSettings = new Settings();
            testSettings.columns = 5;
            testSettings.rows = 4;
            testSettings.startPosition = new Tuple<int, int>(0, 1);
            testSettings.startDirection = North.ToString();
            testSettings.exitPosition = new Tuple<int, int>(4, 2);
            testSettings.mineLocations = new List<Tuple<int, int>> {new Tuple<int, int>(1, 1), new Tuple<int, int>(2,2), new Tuple<int, int>(3, 2)};

            Tuple<Status, string>[] testValues =
             {
                Tuple.Create(Safe, "mrmmmmrmm"),            // Turtle successfully makes it to the Exit.
                Tuple.Create(Safe,  "rrmmrrrmmmmrrrm"),     // Turtle successfully makes it to the Exit.
                Tuple.Create(Out_Of_Bounds,  "mrmmmmm"),    // Turtle wanders out of bounds.
                Tuple.Create(Still_In_Danger,  "mrmm"),     // Turtle still in danger.
                Tuple.Create(Mine_Hit,  "mrmrm"),           // Turtle hits a mine.
                Tuple.Create(Mine_Hit,  "rrmrrrmm"),        // Turtle hits a mine.
                Tuple.Create(Safe,  "mrmmrmrrrmmrmmrmrmm")  // Turtle successfully makes it to the Exit. 
                                                            //Note: A number of moves are skipped at the end as the Turtle has already escaped. 
            };

            MineBoard board = new MineBoard();

            board.Settings = testSettings;

            int turtleStartX = board.Settings.startPosition.Item1;
            int turtleStartY = board.Settings.startPosition.Item2;
            Direction startDirection;

            if (!Enum.TryParse(board.Settings.startDirection, out startDirection))
            {
                Console.WriteLine("Error parsing start direction!");
                return;
            }

            Console.WriteLine("Unit Tests:");

            // Loop through unit tests
            for (int iii = 0; iii < testValues.Length; iii++)
            {
                Status expectedResult = testValues[iii].Item1;
                board.Moves = new Moves(testValues[iii].Item2);
                
                // Create the turtle
                Turtle turtle = new Turtle(turtleStartX, turtleStartY, startDirection);
                //turtle.Direction = startDirection;

                // Loop through the moves
                foreach (char move in board.Moves.moveString.ToLower())
                {
                    switch (move)
                    {
                        case 'm':
                            {
                                turtle.Move();
                                break;
                            }
                        case 'r':
                            {
                                turtle.Rotate();
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Error - Illegal move!");
                                return;
                            }
                    }
   
                    if (board.CheckStatus(turtle) != Still_In_Danger)
                        break;
                }

                Status endStatus = board.CheckStatus(turtle);

                Console.Write("Unit Test " + (iii + 1).ToString());

                if (expectedResult == endStatus)
                    Console.Write(" passed!");
                else
                    Console.Write(" failed!");

                Console.Write(System.Environment.NewLine);
            }

            Console.WriteLine("----------------------" + System.Environment.NewLine + System.Environment.NewLine);
        }
    }
    /*
     * End Main.
     */




    /* 
     * Turtle Class
     * Provides all functionality and fields relating to the Turtle.
     * Public Properties: Position, Direction
     * Public Methods: Move(), Rotate()
     */
    public class Turtle
    {
        //private Position position;
        //private Direction direction;

        public Position Position
        { get; set; }

        public Direction Direction
        { get; set; }

        public Turtle(int startX, int startY, Direction startDirection)
        {
            Position = new Position(startX, startY);
            Direction = startDirection;
        }

        // Moves the Turtle 1 square in whatever direction the Turtle is facing
        public bool Move()
        {
            switch(Direction)
            {
                case North:
                {
                    --Position.Y;
                    break;
                }
                case East:
                {
                    ++Position.X;
                    break;
                }
                case South:
                {
                    ++Position.Y;
                    break;
                }
                case West:
                {
                    --Position.X;
                    break;
                }
                default:
                {
                    return false;
                }
            }

            return true;
        }

        // Rotate the Turtle right by 90 degrees.  Direction is expressed using Cardinal points (N,S,E,W).
        public bool Rotate()
        {
            switch (Direction)
            {
                case North:
                {
                    Direction = East;
                    break;
                }
                case East:
                {
                    Direction = South;
                    break;
                }
                case South:
                {
                    Direction = West;
                    break;
                }
                case West:
                {
                    Direction = North;
                    break;
                }
                default:
                {
                    return false;
                }
            }

            return true;
        }

        // Returns the Turtle's position in a readable string, using the format of "(<X>,<Y>) <DIRECTION>", e.g. (2,3) North
        public string GetTurtleUpdate()
        {
            string location = Position.X + "," + Position.Y;
            string dir = Direction.ToString();

            return location + " " + dir;
        }
    }
    /*
     * End Turtle Class.
     */



    /* 
    * MineBoard Class
    * Provides all functionality and fields relating to the game's board.
    * Public Properties: Settings, Moves
    * Public Methods: LoadSettings(), LoadMoves()
    * 
    * Settings are loaded from a file 'files/settings.json'. Settings contains information such as
    * the size of the board, the start position of the Turtle, the location of the Exit and the location of mines.
    * The Moves that the Turtle will make during the game are loaded from a file 'files/moves.json'.
    * Validations are performed on both files.
    * This class contains a method CheckStatus, a Turtle object is passed into it and it checks the current status of the game.
    * By writing the code this way, it's easy to create games containing multiple Turtles, if so desired.
    */
    // This class holds the board that the Turtle will navigate
    public class MineBoard
    {
        private Settings settings;
        private Moves moves;
        const string settingsFile = "files/settings.json";
        const string movesFile = "files/moves.json";

        public Settings Settings
        { get; set;}

        public Moves Moves
        {get; set;}

        // Load board settings from file. Board size, start position and direction of Turtle, location of Exit, location of Mines.
        public bool LoadSettings()
        {
            Settings fileSettings = null;
            if (!FileUtils.ReadJsonFile(settingsFile, out fileSettings))
            {
                Console.WriteLine("Error loading settings file!");
                return false;
            }

            Settings = fileSettings;

            if (!ValidateSettings(Settings))
            {
                Console.WriteLine("Error validating settings file!");
                return false;
            }

            return true;
        }

        // Load moves that the Turtle will make. This will be a string consisting of Ms (Move) and Rs (Rotate).
        public bool LoadMoves()
        {
            Moves fileMoves;
            if (!FileUtils.ReadJsonFile(movesFile, out fileMoves))
            {
                Console.WriteLine("Error loading moves file!");
                return false;
            }

            Moves = fileMoves;

            if (!ValidateMoves(Moves))
            {
                Console.WriteLine("Error validating moves file!");
                return false;
            }

            return true;
        }

        // Validate that the settings file contains a settings object. Validate that the Turtle's start and exit positions are valid.
        // Validated that the Turtle's start direction is valid.
        private bool ValidateSettings(Settings settings)
        {
            List<string> validDirections = new List<String> { "north", "east", "south", "west" };

            if (settings == null)
                return false;

            // Verify that the game boards has at least one square
            if (settings.columns < 1 || settings.rows < 1)
                return false;

            // Verify that the start position is valid
            if (settings.startPosition.Item1 < 0 || settings.startPosition.Item2 < 0)
                return false;
            else if (settings.startPosition.Item1 > settings.columns || settings.startPosition.Item2 > settings.rows)
                return false;

            // Verify that the exit position is valid
            if (settings.exitPosition.Item1 < 0 || settings.exitPosition.Item2 < 0)
                return false;
            else if (settings.startPosition.Item1 > settings.columns || settings.startPosition.Item2 > settings.rows)
                return false;

            // Verify that start direction is valid
            if (String.IsNullOrEmpty(settings.startDirection) || !validDirections.Contains(settings.startDirection.ToLower()))
                return false;

            return true;
        }

        // Validate that the moves file contains values and only contains valid values.
        private bool ValidateMoves(Moves moves)
        {
            string moveString = moves.moveString.ToLower();

            if (moveString == null || moveString.Length < 1)
                return false;

            foreach (char c in moveString)
            {
                if (c != 'm' && c != 'r')
                    return false;
            }

            return true;
        }

        // Checks the current status of a Turtle. I.e. checks if the Turtle has gone out of bounds, has hit a mine, is safe or is still in danger.
        public Status CheckStatus(Turtle turtle)
        {
            int turtleX = turtle.Position.X;
            int turtleY = turtle.Position.Y;

            if (turtle.Position.X < 0 || turtle.Position.Y < 0)
            {
                return Out_Of_Bounds;
            }
            else if (turtleX >= Settings.columns || turtleY >= Settings.rows)
            {
                return Out_Of_Bounds;
            }
            else if (Settings.mineLocations.Contains(new Tuple<int,int>(turtleX, turtleY)))
            {
                return Mine_Hit;
            }
            else if (Settings.exitPosition.Item1 == turtleX && Settings.exitPosition.Item2 == turtleY)
            {
                return Safe;
            }
            else
            {
                return Still_In_Danger;
            }
        }
    }
    /*
     * End MineBoard class.
     */


    
    /*
     * Definitions of new data types in the form of new Classes and Enums.
     */
    public class Position
    {
        public int X;
        public int Y;

        public Position(int value1, int value2)
        {
            X = value1;
            Y = value2;
        }
    }

    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public enum Status
    {
        Still_In_Danger,
        Out_Of_Bounds,
        Mine_Hit,
        Safe,
        Error
    }

    public class Settings
    {
        public int columns;
        public int rows;
        public Tuple<int, int> startPosition;
        public string startDirection;
        public Tuple<int, int> exitPosition;
        public List<Tuple<int, int>> mineLocations;
    }

    public class Moves
    {
        public string moveString;

        public Moves(string value)
            {
                moveString = value;
            }
    }
    /*
     * End definition of Classes and Enums
     */



   /*
    * FileUtils contains methods used for reading / writing from files.
    */
    // This generic class reads data from a json file and dynamically creates an object of Type T.
    public class FileUtils
    {
        public static bool ReadJsonFile<T>(string filename, out T obj)
        {
            obj = default(T);

            if (filename == null || filename.Length < 1)
                return false;

            try
            {
                using (StreamReader r = new StreamReader(filename))
                {
                    string json = r.ReadToEnd();
                    obj = JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }

            return true;
        }
    }
    /*
     * End FileUtils.f
     */
}
