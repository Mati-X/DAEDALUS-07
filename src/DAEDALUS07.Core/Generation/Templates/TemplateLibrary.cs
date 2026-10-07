using DAEDALUS07.Core.Generation;

namespace DAEDALUS07.Core.Generation.Templates;

public static class TemplateLibrary
{
    public static readonly MacroRoomTemplate SpawnAirlock = new(
        "SpawnAirlock",
        RoomType.Spawn,
        macroLayers: [
            [
                "T"
            ]
        ]
    );

    public static readonly MacroRoomTemplate CatwalkMegaHall = new(
        "CatwalkMegaHall",
        RoomType.Combat,
        macroLayers: [
            [
                "F F F",
                "S D L"
            ],
            [
                "C C C",
                "S    "
            ]
        ]
    );

    public static readonly MacroRoomTemplate PillaredCathedral = new(
        "PillaredCathedral",
        RoomType.Combat,
        macroLayers: [
            [
                "D D",
                "F T"
            ]
        ]
    );

    public static readonly MacroRoomTemplate IndustrialDepot = new(
        "IndustrialDepot",
        RoomType.Combat,
        macroLayers: [
            [
                "D T D",
                "F L F"
            ]
        ]
    );

    public static readonly List<MacroRoomTemplate> AllTemplates = [
        SpawnAirlock,
        CatwalkMegaHall,
        PillaredCathedral,
        IndustrialDepot
    ];

    public static MacroRoomTemplate GetRandomTemplate(RoomType type, int maxW, int maxH, Random random)
    {
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