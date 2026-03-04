using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace EpicGamesBot;

public class DiscordService
{
    private readonly IEpicGamesService _epicGamesService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public DiscordService(IEpicGamesService epicGamesService, HttpClient httpClient, IConfiguration configuration)
    {
        _epicGamesService = epicGamesService;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendDiscordMessage()
    {
        var webhookUrl = _configuration["Discord:WebhookUrl"] ?? throw new InvalidOperationException("Discord:WebhookUrl não configurado.");
        var games = await _epicGamesService.GetFreeGames();
        var embeds = new List<object>();

        foreach (var game in games)
        {
            embeds.Add(new
            {
                title = game.Name,
                description = string.IsNullOrWhiteSpace(game.Link) 
                                ? game.description
                                : $"🔗 [Resgatar aqui]({game.Link})\n\n{game.description}",
                image = new
                {
                    url = game.Image
                },
                color = 5793266, // azul
                timestamp = DateTime.UtcNow
            });
        }

        var payload = new
        {
            content = "🎮 Os Jogos Consolidados dessa semana https://store.epicgames.com/pt-BR/free-games 🎮",
            embeds = embeds
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        await _httpClient.PostAsync(webhookUrl, content);
    }
}
