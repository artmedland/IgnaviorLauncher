using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IgnaviorLauncher.Services;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System;

namespace IgnaviorLauncher.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ManifestService manifest;
    private readonly LocalGameService gameService;

    [ObservableProperty]
    private ObservableCollection<GameViewModel> games;

    private GameViewModel selectedGame;
    public GameViewModel SelectedGame
    {
        get => selectedGame;
        set
        {
            if (SetProperty(ref selectedGame, value) && value != null)
            {
                SelectGameCommand.Execute(value);
            }
        }
    }

    public MainViewModel()
    {
        manifest = new();
        // WARN: Hard-coded to %UserPath%/.Ignavior/
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".Ignavior");
        gameService = new(path);

        Games = new ObservableCollection<GameViewModel>();
        _ = Async();
    }

    private async Task Async()
    {
        var manifest = await this.manifest.FetchManifestAsync();
        if (manifest == null)
        {
            // throw?
            return;
        }

        var games = gameService.GetInstalledGameIds().ToDictionary(id => id);
        var models = new ObservableCollection<GameViewModel>();

        foreach (var pair in manifest.Games)
        {
            string id = pair.Key;
            var game = pair.Value;

            string version = null;
            if (games.ContainsKey(id))
            {
                var info = gameService.GetGameInfo(id);
                version = info?.InstalledVersion;
            }

            string buttonText = "Install";
            if (version != null)
            {
                bool latest = version == game.LatestVersion;
                buttonText = latest ? "Play" : "Update";
            }

            var model = new GameViewModel
            {
                Name = game.Name,
                InstalledVersion = version ?? "",
                TextState = buttonText,

                LastPlayed = games.ContainsKey(id) ? 
                gameService.GetGameInfo(id)?.LastPlayed ?? 
                DateTime.MinValue : DateTime.MinValue
            };

            manifestMap[id] = game;
            models.Add(model);
        }

        Games = [.. models.OrderByDescending(g => g.LastPlayed)];
        SelectedGame = Games.FirstOrDefault();
    }

    private readonly Dictionary<string, GameManifest> manifestMap = new();

    private async void LoadChangelogForGame(GameViewModel game)
    {
        var entry = manifestMap.FirstOrDefault(pair => pair.Value.Name == game.Name);
        if (entry.Key == null)
            return;

        string id = entry.Key;
        var manifest = entry.Value;

        game.PatchNotes.Clear();
        string changelogUrl = "//";
        var versions = new List<string>();

        if (manifest.Base != null)
            versions.Add(manifest.Base.Version);
        foreach (var patch in manifest.Patches)
        {
            if (!versions.Contains(patch.OldVersion))
                versions.Add(patch.OldVersion);
            if (!versions.Contains(patch.NewVersion))
                versions.Add(patch.NewVersion);
        }
        versions = versions.Distinct().OrderByDescending(v => v).ToList();

        foreach (var version in versions)
        {
            string url = $"{changelogUrl}{id}/{version}.md";
            try
            {
                using var client = new System.Net.Http.HttpClient();
                string markdown = await client.GetStringAsync(url);
                game.PatchNotes.Add(new PatchNoteViewModel
                {
                    Version = version,
                    MarkdownContent = markdown,
                    ReleaseDate = DateTime.MinValue
                });
            } 
            catch
            {
                game.PatchNotes.Add(new PatchNoteViewModel
                {
                    Version = version,
                    MarkdownContent = $"#{version}\n",
                    ReleaseDate = DateTime.MinValue
                });
            }
        }
    }

    [RelayCommand]
    private void PrimaryAction(GameViewModel game)
    {
        switch (game.TextState)
        {
            case "Install":
                InstallGame(game);
                break;
            case "Update":
                UpdateGame(game);
                break;
            case "Play":
                PlayGame(game);
                break;
        }
    }

    private void InstallGame(GameViewModel game)
    {

    }

    private void UpdateGame(GameViewModel game)
    {

    }

    private void PlayGame(GameViewModel game)
    {
        var entry = manifestMap.FirstOrDefault(pair => pair.Value.Name == game.Name);
        
        if (entry.Key != null)
        {
            gameService.UpdateLastPlayed(entry.Key);
        }
        game.LastPlayed = DateTime.Now;

        var reordered = Games.OrderByDescending(g => g.LastPlayed).ToList();
        Games.Clear();

        foreach (var g in reordered)
        {
            Games.Add(g);
        }
        SelectedGame = game;
    }

    [Obsolete]
    private void LoadDummyData()
    {
        var games = new ObservableCollection<GameViewModel>();

        var game1 = new GameViewModel
        {
            Name = "Alpha Game",
            InstalledVersion = "v1.2.3",
            TextState = "Play"
        };
        game1.PatchNotes.Add(new PatchNoteViewModel
        {
            Version = "1.2.3",
            ReleaseDate = new DateTime(2026, 3, 14),
            MarkdownContent = @"## **New Features**
- Added support for new control system
- Improved rendering performance

### **Bugs**
- Fixed bugs"
        });
        game1.PatchNotes.Add(new PatchNoteViewModel
        {
            Version = "1.2.2",
            ReleaseDate = new DateTime(2026, 3, 13),
            MarkdownContent = @"- Nothing interesting"
        });
        game1.PatchNotes.Add(new PatchNoteViewModel()
        {
            Version = "1.2",
            ReleaseDate = new DateTime(2026, 1, 23),
            MarkdownContent = @"## Bugs
- Added new bugs
- Fixed none

### New Features
- Major new content
- Patched game-breaking glitch
- New backend for input management
- New version control system
- Removed all old saves"
        });

        var game2 = new GameViewModel
        {
            Name = "Beta Game",
            InstalledVersion = "",
            TextState = "Install"
        };
        game2.PatchNotes.Add(new PatchNoteViewModel
        {
            Version = "v0.9",
            ReleaseDate = new DateTime(2023, 9, 2),
            MarkdownContent = @"## New Features
- Added new level, _The Backrooms_
- Beta release"
        });

        games.Add(game1);
        games.Add(game2);

        Games = [.. games.OrderByDescending(g => g.LastPlayed)];
        SelectedGame = Games.FirstOrDefault();
    }

    [RelayCommand]
    private void SelectGame(GameViewModel game)
    {
        if (game == null)
            return;

        SelectedGame = Games.FirstOrDefault(g => g == game);
    }
}