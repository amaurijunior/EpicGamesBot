using System.Text.Json;
using Microsoft.SemanticKernel;

namespace EpicGamesBot;

public interface IEpicGamesService
{
    Task<List<Game>> GetFreeGames();
}

public class EpicGamesService : IEpicGamesService
{
    private readonly HttpClient _httpClient;

    public EpicGamesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Game>> GetFreeGames()
    {
        var url = "https://store-site-backend-static-ipv4.ak.epicgames.com/freeGamesPromotions" +
                  "?locale=pt-BR&country=BR&allowCountries=BR&start=0&count=1000";

        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        var elements = json.RootElement
            .GetProperty("data")
            .GetProperty("Catalog")
            .GetProperty("searchStore")
            .GetProperty("elements")
            .EnumerateArray();

        var now = DateTime.UtcNow;
        var freeGames = new List<Game>();

        foreach (var game in elements)
        {
            if (!game.TryGetProperty("promotions", out var promotions) || 
                promotions.ValueKind == JsonValueKind.Null)
                continue;

            if (!promotions.TryGetProperty("promotionalOffers", out var offerGroups))
                continue;

            foreach (var group in offerGroups.EnumerateArray())
            {
                if (!group.TryGetProperty("promotionalOffers", out var offers))
                    continue;

                foreach (var offer in offers.EnumerateArray())
                {
                    var startDate = offer.GetProperty("startDate").GetString();
                    var endDate = offer.GetProperty("endDate").GetString();
                    var discount = offer
                        .GetProperty("discountSetting")
                        .GetProperty("discountPercentage")
                        .GetInt32();

                    if (startDate == null || endDate == null) continue;

                    var start = DateTime.Parse(startDate).ToUniversalTime();
                    var end = DateTime.Parse(endDate).ToUniversalTime();

                    if (start <= now && now <= end && discount == 0)
                    {
                        var title = game.GetProperty("title").GetString() ?? "N/A";
                        var desc = game.GetProperty("description").GetString() ?? "";
                        var slug = game.TryGetProperty("productSlug", out var s) ? s.GetString() : "";
                        var gameUrl = slug != null ? $"https://store.epicgames.com/pt-BR/p/{slug}" : null;

                        string? imageUrl = null;
                        if (game.TryGetProperty("keyImages", out var images))
                        {
                            foreach (var img in images.EnumerateArray())
                            {
                                if (img.GetProperty("type").GetString() == "Thumbnail")
                                {
                                    imageUrl = img.GetProperty("url").GetString();
                                    break;
                                }
                            }
                        }

                        freeGames.Add(new Game(title, gameUrl, imageUrl, desc));
                    }
                }
            }
        }

        return freeGames;
    }
}

