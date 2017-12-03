using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Game
    {
        //game parammeters
        const int TetrisWidth = 10;
        const int TetrisHeight = 16;
        const int GameWidth = TetrisWidth + 3;
        const int GameHeight = TetrisHeight + 2;
        const char BorderCharacter = '*';
        int removedLines = 0;
        static int penalty;
        #region Figures
        //matrix of boolean for the block
        static bool[][,] Figures = new bool[2][,]
        {
        new bool[,]
        {
            { true,}
        },
        new bool[,]
        {
            { true, true },
            { true, true }
        },
        };
        #endregion
        static bool[,] currentFigure;
        static int currentFigureRow = 0;
        static int currentFigureCol = 4;
        static bool[,] nextFigure;
        static Random random = new Random();
        static bool[,] gameState = new bool[
            TetrisHeight, TetrisWidth];
        static bool end = false;
        private int serverBlock;

        public bool End
        {
            get { return end; }
        }

        public int RemovedLines { get => removedLines; set => removedLines = value; }
        public int Penalty { get => penalty; set => penalty = value; }
        public int ServerBlock { get => serverBlock; set => serverBlock = value; }

        public void LaunchGame()
        {
            Console.OutputEncoding = Encoding.GetEncoding(1252);
            Console.CursorVisible = false;

            StartNewGame();
            PrintBorders();

            while (end == false)
            {
                GameOver();
                //set the lest and the right movement 
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.LeftArrow)
                    {
                        if (currentFigureCol > 1)
                        {
                            currentFigureCol--;
                        }
                    }
                    else if (key.Key == ConsoleKey.RightArrow)
                    {
                        if (currentFigureCol + currentFigure.GetLength(1) - 1 < TetrisWidth)
                        {
                            currentFigureCol++;
                        }
                    }
                }

                if (CollisionDetection())
                {
                    PlaceCurrentFigure();
                    if (CheckForFullLines() > 0)
                    {
                        removedLines += 1;
                    }
                    //Set the next figure from the number sent by the server
                    currentFigure = nextFigure;
                    //nextFigure = Figures[serverBlock];
                    nextFigure = Figures[random.Next(0, Figures.Length)];
                    currentFigureRow = 1;
                    currentFigureCol = 4;
                }
                else
                {
                    currentFigureRow++;
                }
                //keep printing the game field with the new position of the block
                PrintGameField();

                PrintBorders();
                Console.SetCursorPosition(0, TetrisWidth+8);
                Console.WriteLine("Lines removed : "+removedLines);
                PrintFigure(currentFigure,
                    currentFigureRow, currentFigureCol);
                
                Thread.Sleep(200);
            }
        }
        public int CheckForFullLines()
        {
            int linesRemoved = 0;
            // if the a line of the board have all items on true  mean it is a filled line
            for (int row = 0; row < gameState.GetLength(0); row++)
            {
                bool isFullLine = true;
                for (int col = 0; col < gameState.GetLength(1); col++)
                {
                    if (gameState[row, col] == false)
                    {
                        isFullLine = false;
                        break;
                    }
                }

                if (isFullLine)
                {//if the line is filled set it to false(erase all items) and bring down the line from above
                    for (int nextLine = row - 1; nextLine >= 0; nextLine--)
                    {
                        if (row < 0)
                        {
                            continue;
                        }

                        for (int colFromNextLine = 0; colFromNextLine < gameState.GetLength(1); colFromNextLine++)
                        {
                            gameState[nextLine + 1, colFromNextLine] =
                                gameState[nextLine, colFromNextLine];
                        }
                    }

                    for (int colLastLine = 0; colLastLine < gameState.GetLength(1); colLastLine++)
                    {
                        gameState[0, colLastLine] = false;
                    }

                    linesRemoved++;
                }
            }

            return linesRemoved;
        }

        static void PlaceCurrentFigure()
        {
            for (int figRow = 0; figRow < currentFigure.GetLength(0); figRow++)
            {
                for (int figCol = 0; figCol < currentFigure.GetLength(1); figCol++)
                {
                    var row = currentFigureRow - 1 + figRow;
                    var col = currentFigureCol - 1 + figCol;

                    if (currentFigure[figRow, figCol])
                    {
                        gameState[row, col] = true;
                    }
                }
            }
        }

        static bool CollisionDetection()
        {
            var currentFigureLowestRow =
                currentFigureRow +
                currentFigure.GetLength(0);

            if (currentFigureLowestRow > TetrisHeight)
            {
                return true;
            }

            for (int figRow = 0; figRow < currentFigure.GetLength(0); figRow++)
            {
                for (int figCol = 0; figCol < currentFigure.GetLength(1); figCol++)
                {
                    var row = currentFigureRow + figRow;
                    var col = currentFigureCol - 1 + figCol;

                    if (row < 0)
                    {
                        continue;
                    }

                    if (gameState[row, col] == true &&
                        currentFigure[figRow, figCol] == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// set all filled with blank if there is no block, whith '#' otherwise
        /// </summary>
        static void PrintGameField()
        { 
            for (int row = 1; row <= TetrisHeight-penalty; row++)
            {
                for (int col = 1; col <= TetrisWidth; col++)
                {
                    if (gameState[row - 1, col - 1] == true)
                    {
                        Print(row, col, '#');
                    }
                    else
                    {
                        Print(row, col, ' ');
                    }
                }
            }
        }


        static void PrintFigure(bool[,] figure, int row, int col)
        {
            for (int x = 0; x < figure.GetLength(0); x++)
            {
                for (int y = 0; y < figure.GetLength(1); y++)
                {
                    if (figure[x, y] == true)
                    {
                        Print(row + x, col + y, '#');
                    }
                }
            }
        }
        /// <summary>
        /// The two first figures are not from the Server
        /// </summary>
        static void StartNewGame()
        {
            currentFigure = Figures[
                random.Next(0, Figures.Length)];
            nextFigure = Figures[
                random.Next(0, Figures.Length)];
        }



        static void PrintBorders()
        {
            for (int col = 0; col < GameWidth; col++)
            {
                Print(0, col, BorderCharacter);
                Print(GameHeight - 1, col, BorderCharacter);
            }

            for (int row = 0; row < GameHeight-penalty; row++)
            {
                Print(row, 0, BorderCharacter);
                Print(row, TetrisWidth + 1, BorderCharacter);
            }
        }

        static void Print(int row, int col, object data)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(col, row);
            Console.Write(data);
        }
        /// <summary>
        /// if a block touch the top of the field its game over
        /// </summary>
        static void GameOver()
        {
            for (int col = 0; col < TetrisWidth; col++)
            {
                if (gameState[0, col] == true)
                {
                    end = true;
                    break;
                }
            }
        }
    }
}

