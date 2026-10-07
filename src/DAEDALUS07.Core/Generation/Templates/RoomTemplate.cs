using DAEDALUS07.Core.Generation;

namespace DAEDALUS07.Core.Generation.Templates;

public class RoomTemplate(string name, RoomType type, string[][] layers)
{
    public string Name { get; } = name;
    public RoomType Type { get; } = type;
    public string[][] Layers { get; } = layers;
    public int Width => Layers.Length > 0 && Layers[0].Length > 0 ? Layers[0][0].Length : 0;
    public int Height => Layers.Length > 0 ? Layers[0].Length : 0;

    public static RoomTemplate FromFile(string filePath, RoomType defaultType = RoomType.Combat)
    {
        string name = Path.GetFileNameWithoutExtension(filePath);
        string content = File.ReadAllText(filePath);

        string[] rawLayers = content.Split(["---"], StringSplitOptions.None);
        List<List<string>> parsedLayers = [];

        foreach (var rawLayer in rawLayers)
        {
            var lines = rawLayer.Split(["\r\n", "\n"], StringSplitOptions.None)
                .Where(line => !line.TrimStart().StartsWith('#'))
                .ToList();

            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0])) lines.RemoveAt(0);
            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1])) lines.RemoveAt(lines.Count - 1);

            if (lines.Count > 0)
            {
                parsedLayers.Add(lines);
            }
        }

        if (parsedLayers.Count == 0)
        {
            return new RoomTemplate(name, defaultType, [["."]]);
        }

        int maxWidth = parsedLayers.Max(l => l.Max(line => line.Length));
        int maxHeight = parsedLayers.Max(l => l.Count);

        string[][] finalLayers = new string[parsedLayers.Count][];
        for (int l = 0; l < parsedLayers.Count; l++)
        {
            finalLayers[l] = new string[maxHeight];
            for (int r = 0; r < maxHeight; r++)
            {
                string original = r < parsedLayers[l].Count ? parsedLayers[l][r] : "";
                finalLayers[l][r] = original.PadRight(maxWidth, ' ');
            }
        }

        return new RoomTemplate(name, defaultType, finalLayers);
    }
}