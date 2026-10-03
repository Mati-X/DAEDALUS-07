
using System.Text.Json;
using SadRogue.Primitives;

public class Theme
{
    public static Theme Current { get; private set; } = CreateDefault();

    public Color WallFront { get; init; }
    public Color WallRoof { get; init; }
    public Color WallDim { get; init; }
    public Color FloorLit { get; init; }
    public Color FloorFog { get; init; }
    public Color Player { get; init; }
    public Color Enemy { get; init; }
    
    private static Theme CreateDefault() => new()
    {
        WallFront = Color.Crimson,
        WallRoof = Color.Red,
        WallDim = new Color(30, 10, 15,255),
        FloorLit = Color.White,
        FloorFog = Color.Black,
        Player = Color.Yellow,
        Enemy = Color.Red
    };

    public static void Load(string filePath)
    {
        if (!File.Exists(filePath)) return;

        string json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Color primary = FromHex(root.GetProperty("primary").GetString()!);
        Color floor = FromHex(root.GetProperty("floor").GetString()!);

        Current = new Theme
        {
            WallFront = primary,
            WallRoof = Color.Lerp(Color.Black, primary, 0.50f),
            WallDim = Color.Lerp(Color.Black, primary, 0.05f),

            FloorLit = floor,
            FloorFog = Color.Lerp(Color.Black, floor, 0.05f),

            Player = FromHex(root.GetProperty("player").GetString()!),
            Enemy = FromHex(root.GetProperty("enemy").GetString()!)
        };
    }
    
    private static Color FromHex(string hex)
    {
        hex = hex.TrimStart('#');
        byte r = Convert.ToByte(hex[..2], 16);
        byte g = Convert.ToByte(hex[2..4], 16);
        byte b = Convert.ToByte(hex[4..6], 16);
        return new Color(r, g, b);
    }
}