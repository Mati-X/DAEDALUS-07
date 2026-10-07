namespace DAEDALUS07.Core.Generation.Templates;

public static class ModuleLibrary
{
    public const int ModuleSize = 10;

    // =========================================================================
    // MODUŁY: 'F' - OTWARTA PODŁOGA (FLOOR / COMBAT ARENA)
    // =========================================================================
    private static readonly List<string[]> FloorVariants = [
        // F1: Czysta posadzka
        [
            "..........",
            "..........",
            "..........",
            "..........",
            "..........",
            "..........",
            "..........",
            "..........",
            "..........",
            ".........."
        ],
        // F2: Posadzka z centralną małą osłoną
        [
            "..........",
            "..........",
            "....░░....",
            "....░░....",
            "..........",
            "..........",
            "....░░....",
            "....░░....",
            "..........",
            ".........."
        ]
        // TODO: Dodaj tutaj kolejny wariant otwartej podłogi 'F' (np. kable, drobne przeszkody)
    ];

    // =========================================================================
    // MODUŁY: 'D' - DEKORACJE I FILARY (PILLARS / INDUSTRIAL COVER)
    // =========================================================================
    private static readonly List<string[]> DecorationVariants = [
        // D1: Cztery kolumny w narożnikach
        [
            "..O....O..",
            "..O....O..",
            "..........",
            "..........",
            "....░░....",
            "....░░....",
            "..........",
            "..........",
            "..O....O..",
            "..O....O.."
        ],
        // D2: Masywny podwójny filar w centrum z osłonami
        [
            "..........",
            "...░░░░...",
            "...░OO░...",
            "...░OO░...",
            "...░░░░...",
            "...░░░░...",
            "...░OO░...",
            "...░OO░...",
            "...░░░░...",
            ".........."
        ]
        // TODO: Dodaj tutaj kolejny wariant filarów 'D' (np. rzędy serwerów, rury zasilające)
    ];

    // =========================================================================
    // MODUŁY: 'C' - KŁADKI (CATWALK)
    // =========================================================================
    private static readonly List<string[]> CatwalkVariants = [
        // C1: Kładka pozioma u góry z barierką
        [
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "░░░░░░░░░░",
            "          ",
            "          ",
            "          ",
            "          ",
            "          ",
            "          "
        ],
        // C2: Pełny podest kładki
        [
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "░░░░░░░░░░",
            "          ",
            "          "
        ]
        // TODO: Dodaj tutaj kolejny wariant kładki 'C' (np. zakręt L, kładka pionowa)
    ];

    // =========================================================================
    // MODUŁY: 'S' - SCHODY (STAIRS / LEVEL CONNECTOR)
    // =========================================================================
    private static readonly List<string[]> StairsVariants = [
        // S1: Kładka z zejściem schodami na dół
        [
            "≡≡≡≡≡≡≡≡≡≡",
            "≡≡≡≡≡≡≡≡≡≡",
            "░░░░==░░░░",
            "    ==    ",
            "..........",
            "..........",
            "..........",
            "..........",
            "..........",
            ".........."
        ]
        // TODO: Dodaj tutaj kolejny wariant schodów 'S' (np. schody boczne, rampa dwukierunkowa)
    ];

    // =========================================================================
    // MODUŁY: 'T' - TERMINAL (CENTRAL TERMINAL / CORE)
    // =========================================================================
    private static readonly List<string[]> TerminalVariants = [
        // T1: Terminal na środku otoczony osłonami
        [
            "..........",
            "..........",
            "...░░░░...",
            "...░..░...",
            "...░.T░...",
            "...░..░...",
            "...░░░░...",
            "..........",
            "..........",
            ".........."
        ]
        // TODO: Dodaj tutaj kolejny wariant terminala 'T' (np. podwójny mainframe, terminal przy ścianie)
    ];

    // =========================================================================
    // MODUŁY: 'L' - LOOT / MAGAZYN (SUPPLY CRATES / VAULT)
    // =========================================================================
    private static readonly List<string[]> LootVariants = [
        // L1: Stanowisko zaopatrzeniowe z osłonami
        [
            "..........",
            "..░░░░░░..",
            "..░....░..",
            "..░.░░.░..",
            "..░.░░.░..",
            "..░....░..",
            "..░░░░░░..",
            "..........",
            "..........",
            ".........."
        ]
        // TODO: Dodaj tutaj kolejny wariant skarbca 'L' (np. skrzynie w rogach, magazyn amunicji)
    ];

    // Pusty moduł (powietrze / void)
    private static readonly string[] EmptyModule = Enumerable.Repeat("          ", ModuleSize).ToArray();

    public static string[] GetRandomVariant(char moduleCode, Random random)
    {
        return moduleCode switch
        {
            'F' => FloorVariants[random.Next(FloorVariants.Count)],
            'D' => DecorationVariants[random.Next(DecorationVariants.Count)],
            'C' => CatwalkVariants[random.Next(CatwalkVariants.Count)],
            'S' => StairsVariants[random.Next(StairsVariants.Count)],
            'T' => TerminalVariants[random.Next(TerminalVariants.Count)],
            'L' => LootVariants[random.Next(LootVariants.Count)],
            ' ' => EmptyModule,
            _   => FloorVariants[0] // fallback: zwykła podłoga
        };
    }
}
