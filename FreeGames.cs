using System.Text.Json.Serialization;

namespace EpicGamesBot;
public record Game(string Name, string? Link, string Image, string description);

public class FreeGames
{
    public List<Game> Games { get; set; }
    public DateTime ExpirationDate { get; set; }
}
