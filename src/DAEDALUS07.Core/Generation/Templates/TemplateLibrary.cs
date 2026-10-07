using DAEDALUS07.Core.Generation;

namespace DAEDALUS07.Core.Generation.Templates;

public static class TemplateLibrary
{
    public static readonly RoomTemplate SpawnAirlock = new(
        "SpawnAirlock",
        RoomType.Spawn,
        [
            [
                "................",
                "................",
                "....O......O....",
                ".......░░.......",
                ".......░T.......",
                ".......░░.......",
                "....O......O....",
                "................",
                "................",
                "................"
            ]
        ]
    );

    public static readonly RoomTemplate GrandCatwalkVault = new(
        "GrandCatwalkVault",
        RoomType.Combat,
        [
            [
                "..........................",
                "..........................",
                "....O................O....",
                ".......░░░......░░░.......",
                ".......░T░......░.░.......",
                ".......░░░......░░░.......",
                "..........................",
                "..........................",
                "....O................O....",
                ".......░░░......░░░.......",
                ".......░.░......░.░.......",
                ".......░░░......░░░.......",
                "..........................",
                ".........................."
            ],
            [
                "≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡",
                "≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡",
                "░░░░░░░░░░░░==░░░░░░░░░░░░",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡",
                "≡≡░                      ░≡"
            ]
        ]
    );

    public static readonly List<RoomTemplate> AllTemplates = [
        SpawnAirlock,
        GrandCatwalkVault
    ];

    private static bool _loadedFromDisk;

    public static void LoadFromDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath)) return;

        foreach (var file in Directory.GetFiles(directoryPath, "*.txt"))
        {
            try
            {
                RoomType type = Path.GetFileName(file).StartsWith("Spawn", StringComparison.OrdinalIgnoreCase) 
                    ? RoomType.Spawn 
                    : (Path.GetFileName(file).StartsWith("Boss", StringComparison.OrdinalIgnoreCase) ? RoomType.BossVault : RoomType.Combat);

                var template = RoomTemplate.FromFile(file, type);
                // Unikamy duplikatów o tej samej nazwie
                if (AllTemplates.All(t => t.Name != template.Name))
                {
                    AllTemplates.Add(template);
                }
            }
            catch
            {
                // ignorujemy uszkodzony plik
            }
        }
    }

    public static RoomTemplate GetRandomTemplate(RoomType type, int maxW, int maxH, Random random)
    {
        if (!_loadedFromDisk)
        {
            LoadFromDirectory("Templates");
            LoadFromDirectory(Path.Combine(AppContext.BaseDirectory, "Templates"));
            _loadedFromDisk = true;
        }

        var matching = AllTemplates
            .Where(t => t.Type == type && t.Width <= maxW && t.Height <= maxH)
            .ToList();

        if (matching.Count == 0)
        {
            var anyOfType = AllTemplates.Where(t => t.Type == type).ToList();
            if (anyOfType.Count > 0) return anyOfType[random.Next(anyOfType.Count)];
            return SpawnAirlock;
        }

        return matching[random.Next(matching.Count)];
    }
}