# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the application
dotnet run

# Build
dotnet build

# Restore dependencies
dotnet restore
```

The app runs on `http://localhost:5053` (or `https://localhost:7025`) and immediately fires the Discord webhook on startup — there are no HTTP endpoints to call manually.

## Architecture

This is a .NET 10 ASP.NET Core Web app that runs as a one-shot bot: on `ApplicationStarted`, it fetches the current Epic Games free games and posts them to a Discord channel via webhook.

**Flow:**
1. `Program.cs` — wires DI and registers the `ApplicationStarted` hook that calls `DiscordService.SendDiscordMessage()`
2. `EpicGamesService` — uses **Microsoft Semantic Kernel** to invoke an LLM prompt (via OpenRouter) that browses the Epic Games store and returns a JSON list of free games
3. `Constants.PROMPT` — the Portuguese-language prompt instructing the LLM to scrape `store.epicgames.com/pt-BR/` and return structured JSON
4. `FreeGames` / `Game` — record types that the JSON response is deserialized into
5. `DiscordService` — builds a Discord embed from the deserialized games and POSTs it to a hardcoded webhook URL

**Key config (`appsettings.json`):**
- `OpenAI:ApiKey` — OpenRouter API key used by Semantic Kernel (configured with OpenRouter's endpoint `https://openrouter.ai/api/v1` and model `z-ai/glm-4.5-air:free`)

**Note:** The Discord webhook URL and OpenAI/OpenRouter API key are currently hardcoded in `DiscordService.cs` and `appsettings.json` respectively. Move them to `appsettings.Development.json` or user secrets for local dev (`dotnet user-secrets`).
