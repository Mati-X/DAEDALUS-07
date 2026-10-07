using DAEDALUS07.Core.Generation;

namespace DAEDALUS07.Core.Generation.Templates;

public static class TemplateLibrary
{
    // =========================================================================
    // 1. ŚLUZA STARTOWA (2x2 moduły = 20x20 kratek)
    // =========================================================================
    public static readonly MacroRoomTemplate SpawnAirlock = new(
        "SpawnAirlock",
        RoomType.Spawn,
        macroLayers: [
            [
                "F F",
                "F T"
            ]
        ]
    );

    // =========================================================================
    // 2. MONUMENTALNA HALA Z KŁADKĄ I SCHODAMI (5x3 moduły = 50x30 kratek!)
    // =========================================================================
    public static readonly MacroRoomTemplate CatwalkMegaHall = new(
        "CatwalkMegaHall",
        RoomType.Combat,
        macroLayers: [
            // Warstwa 0: Parter hali
            [
                "F F F F F",
                "F D T D F",
                "S F F F L"
            ],
            // Warstwa 1: Kładka nad górną ścianą łącząca się ze schodami po lewej
            [
                "C C C C C",
                "         ",
                "S        "
            ]
        ]
    );

    // =========================================================================
    // 3. KATEDRA FILAROWA (4x3 moduły = 40x30 kratek!)
    // =========================================================================
    public static readonly MacroRoomTemplate PillaredCathedral = new(
        "PillaredCathedral",
        RoomType.Combat,
        macroLayers: [
            [
                "D F F D",
                "F T F F",
                "D F F D"
            ]
        ]
    );

    // =========================================================================
    // 4. WIELKI MAGAZYN PRZEMYSŁOWY (5x4 moduły = 50x40 kratek!)
    // =========================================================================
    public static readonly MacroRoomTemplate IndustrialDepot = new(
        "IndustrialDepot",
        RoomType.Combat,
        macroLayers: [
            [
                "D F F F D",
                "F L T L F",
                "F F F F F",
                "D F F F D"
            ]
        ]
    );

    // TODO: Dodaj tutaj swój kolejny szablon makro! (np. krzyżowy, skarbiec z 3 poziomami itp.)

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