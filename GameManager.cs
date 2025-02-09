using System.Diagnostics;
using DotQueens.Core;
using Spectre.Console;

namespace DotQueens.Game;
public class GameManager
{

    private Board? currentBoard;

    private int cursorX = 0;
    private int cursorY = 0;
    private Stopwatch stopwatch = new Stopwatch();
    public GameManager()
    {
        RenderMainMenu();
    }

    private void RenderMainMenu()
    {
        // Using Spectre.Console for rendering the main menu
        AnsiConsole.Clear();
        // The menu is a panel with a header and a list of options
        var titleText = "♛ Welcome to DotQueens! ♛";
        var titleTextMarkup = new Markup(titleText, new Style(foreground: Color.Red));
        var totalTitleLenght = titleText.Length + 4;
        // Center title text in console
        var titleTextCentered = new Padder(titleTextMarkup).PadLeft((Console.WindowWidth + totalTitleLenght) / 4);
        AnsiConsole.Write(titleTextCentered);
        var selection = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select your option:")
        .AddChoices(new[] {
            "Start Game",
            "About",
            "Exit"
        }));

        if (selection == "Exit")
        {

            ExitGame();
        }
        else if (selection == "About")
        {
            RenderAbout();
        }
        else if (selection == "Start Game")
        {
            RenderLevelSelect();
        }

    }

    private void RenderLevelSelect()
    {
        // Create a prompt to ask the user for the level difficulty
        // The user can choose between easy, medium, and hard
        var difficulty = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Select your level:")
            .AddChoices(new[] { "😊 Easy", "🤨 Medium", "😱 Hard" }));
        StartNewGame(difficulty);
    }

    private void StartNewGame(string difficulty)
    {
        int size = 4;
        if (difficulty == "🤨 Medium")
        {
            size = 6;
        }
        else if (difficulty == "😱 Hard")
        {
            size = 9;
        }
        var board = new Board(size);
        while (!board.IsLevelSolvable())
        {
            board = new Board(size);
        }
        board.ResetQueens();
        currentBoard = board;
        cursorX = 0;
        cursorY = 0;
        stopwatch.Reset();
        stopwatch.Start();
        AnsiConsole.Clear();
        RenderGame();
    }

    private void RenderAbout()
    {
        AnsiConsole.Clear();
        var aboutText = @"
[bold]The rules:[/]
DotQueens is a console-based game where you have to place N queens ♛ on an NxN colored grid. 
Each color must only have one queen, and no queen can be in the same row or column as another queen. 
Queens can also not be next to each other diagonally.
The game is won when all queens are placed on the board without any conflicts. 

[bold]About the game:[/]
Created by :person_raising_hand: Christian Mühle :e_mail: info@devowl.de / :globe_showing_europe_africa: devowl.de 
The game is inspired by Queens from LinkedIn.";
        AnsiConsole.MarkupLine(aboutText);
        var back = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Press Enter to go back")
            .AddChoices(new[] { "Back" }));
        if (back == "Back")
        {
            RenderMainMenu();
        }
    }


    private void RenderGame(bool toggleHelp = false)
    {
        Console.SetCursorPosition(0, 0);
        Console.CursorVisible = false;
        if (currentBoard!.ValidateDetail() == Board.ValidationType.None)
        {
            RenderWinScreen();
        }
        var board = currentBoard;
        var boardSize = board!.Size;
        var canvas = new Canvas(boardSize, boardSize);
        for (int r = 0; r < boardSize; r++)
        {
            for (int c = 0; c < boardSize; c++)
            {
                canvas.SetPixel(r, c, GetColorAt(r, c));
            }
        }
        canvas.Scale = true;
        canvas.PixelWidth = 3;
        AnsiConsole.Write(canvas);
        // Render already placed queens
        var blockedRows = new HashSet<int>();
        var blockedColumns = new HashSet<int>();
        var blockedColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int r = 0; r < boardSize; r++)
        {
            for (int c = 0; c < boardSize; c++)
            {
                if (board.Grid[r, c].HasQueen)
                {
                    RenderQueen(r * 3, c);
                    blockedRows.Add(r);
                    blockedColumns.Add(c);
                    blockedColors.Add(board.Grid[r, c].Color);
                }
            }
        }
        if (toggleHelp)
        {
            // Render an X in each cell that would be invalid
            for (int r = 0; r < boardSize; r++)
            {
                for (int c = 0; c < boardSize; c++)
                {
                    if ((blockedRows.Contains(r) || blockedColumns.Contains(c) || blockedColors.Contains(board.Grid[r, c].Color)) && !board.Grid[r, c].HasQueen)
                    {
                        RenderBlock(r * 3, c);
                    }
                    // If the cell has a queen also render a block in the direct diagonals but not in the same cell
                    if (board.Grid[r, c].HasQueen)
                    {
                        if (r > 0 && c > 0)
                        {
                            RenderBlock((r - 1) * 3, c - 1);
                        }
                        if (r > 0 && c < boardSize - 1)
                        {
                            RenderBlock((r - 1) * 3, c + 1);
                        }
                        if (r < boardSize - 1 && c > 0)
                        {
                            RenderBlock((r + 1) * 3, c - 1);
                        }
                        if (r < boardSize - 1 && c < boardSize - 1)
                        {
                            RenderBlock((r + 1) * 3, c + 1);
                        }
                    }
                }
            }
        }
        // Render the cursor
        MoveCursor(cursorX, cursorY);

        Console.WriteLine("Arrow keys to move, Press Enter to toggle a queen, r to reset, ESC to go back to main menu");
        if (toggleHelp)
        {
            AnsiConsole.MarkupLine("h to toggle help view: [green]Enabled[/]     ");
        }
        else
        {
            AnsiConsole.MarkupLine("h to toggle help view: [red]Disabled[/]      ");
        }
        // Render current validation state
        var validation = board.ValidateDetail();
        var validationText = "";
        switch (validation)
        {
            case Board.ValidationType.RowsAndColumns:
                validationText = "Row/Column conflict!            ";
                break;
            case Board.ValidationType.Adjacency:
                validationText = "Diagonal conflict!              ";
                break;
            case Board.ValidationType.Colors:
                validationText = "At least one Color has no Queen!";
                break;
        }
        if (validation != Board.ValidationType.None)
        {
            AnsiConsole.MarkupLine($"State: [red]{validationText}[/]");
        }
        var result = Console.ReadKey();
        var exit = false;
        switch (result.Key)
        {
            case ConsoleKey.UpArrow:
                if (cursorY > 0)
                {
                    cursorY--;
                }
                break;
            case ConsoleKey.DownArrow:
                if (cursorY < boardSize - 1)
                {
                    cursorY++;
                }
                break;
            case ConsoleKey.LeftArrow:
                if (cursorX >= 3)
                {
                    cursorX -= 3;
                }
                break;
            case ConsoleKey.RightArrow:
                if (cursorX < (boardSize * 3) - 3)
                {
                    cursorX += 3;
                }
                break;
            case ConsoleKey.Enter:
                if (board.Grid[cursorX / 3, cursorY].HasQueen)
                {
                    board.Grid[cursorX / 3, cursorY].HasQueen = false;
                }
                else
                {
                    board.Grid[cursorX / 3, cursorY].HasQueen = true;
                }
                break;
            case ConsoleKey.Escape:
                {
                    exit = true;
                    RenderMainMenu();
                }
                break;
            case ConsoleKey.H:
                {
                    toggleHelp = !toggleHelp;
                    break;
                }
            case ConsoleKey.R:
                {
                    board.ResetQueens();
                    break;
                }
        }
        if (!exit)
        {
            RenderGame(toggleHelp);
        }
    }

    private void RenderBlock(int r, int c)
    {
        if (currentBoard!.Grid[r / 3, c].HasQueen)
        {
            return;
        }
        var color = GetColorAt(r / 3, c);
        var foregroundColor = Color.White;
        // If the color is bright use black as forground color
        if (color.R + color.G + color.B > 500)
        {
            foregroundColor = Color.Black;
        }
        Console.SetCursorPosition(r, c);
        AnsiConsole.Write(new Markup(" • ", new Style(foreground: foregroundColor, background: color)));
    }

    private void RenderQueen(int r, int c, Decoration? decoration = null)
    {
        var color = GetColorAt(r / 3, c);
        var foregroundColor = Color.White;
        // If the color is bright use black as forground color
        if (color.R + color.G + color.B > 500)
        {
            foregroundColor = Color.Black;
        }
        Console.SetCursorPosition(r, c);
        AnsiConsole.Write(new Markup(" ♛ ", new Style(foreground: foregroundColor, background: color, decoration)));
    }
    private void MoveCursor(int r, int c)
    {
        RenderQueen(r, c, Decoration.SlowBlink);
        Console.SetCursorPosition(0, currentBoard!.Size + 1);
    }

    private Color GetColorAt(int r, int c)
    {
        currentBoard!.colorMap.TryGetValue(currentBoard.Grid[r, c].Color, out var color);
        return color;
    }


    private void RenderWinScreen()
    {
        stopwatch.Stop();
        var time = stopwatch.Elapsed.ToString("mm\\:ss\\.ff");
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("Congratulations! You won the game! :birthday_cake: ");
        AnsiConsole.MarkupLine($"Time: {time}");
        var back = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Press Enter to go back")
            .AddChoices(new[] { "Back" }));
        RenderMainMenu();

    }

    private void ExitGame()
    {
        AnsiConsole.MarkupLine("Exiting game... :waving_hand: ");
        Environment.Exit(0);
    }
}