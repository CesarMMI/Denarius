namespace Denarius.Domain.Exceptions.Color;

public class InvalidColorException() : ColorException($"The color is not a valid hex color code.")
{
}