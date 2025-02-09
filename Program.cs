using System;
using System.Collections.Generic;
using DotQueens.Game;





public class Program
{
    public static void Main()
    {
        new GameManager();
        /*var trys = 1;
        var success = false;
        Board? board;
        var timer = new System.Diagnostics.Stopwatch();
        do
        {
            timer.Start();
            board = new Board(9);
            if (board.IsLevelSolvable() && board.Validate())
            {
                success = true;
            }
            else
            {
                Console.WriteLine($"Try: {trys} failed. Retrying...");
                trys++;
            }
        } while (trys < 1000 && success == false);
        timer.Stop();
        Console.WriteLine("Board created successfully. After " + trys + " tries. Time taken: " + timer.ElapsedMilliseconds + "ms");
        board.PrintBoard();
*/
    }
}