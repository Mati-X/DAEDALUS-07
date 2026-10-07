namespace DAEDALUS07.Core.Generation.Templates;

public static class ModuleLibrary
{
    public const int ModuleSize = 10;

    private static readonly List<string[]> FloorVariants = [
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
    ];

    private static readonly List<string[]> DecorationVariants = [
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
    ];

    private static readonly List<string[]> CatwalkVariants = [
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
    ];

    private static readonly List<string[]> StairsVariants = [
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
    ];

    private static readonly List<string[]> TerminalVariants = [
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
    ];

    private static readonly List<string[]> LootVariants = [
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
    ];

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
            _   => FloorVariants[0]
        };
    }
}
