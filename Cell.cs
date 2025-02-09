namespace DotQueens.Core;
public class Cell(string color)
{
    public string Color { get; set; } = color;
    public bool HasQueen { get; set; } = false;
}