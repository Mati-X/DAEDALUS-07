using DAEDALUS07.Core.Generation;

namespace DAEDALUS07.Core.Generation.Templates;

public class MacroRoomTemplate
{
    public string Name { get; }
    public RoomType Type { get; }
    public string[][] MacroLayers { get; }

    public int MacroRows => MacroLayers.Length > 0 ? MacroLayers[0].Length : 0;
    public int MacroCols { get; }

    public int Width => MacroCols * ModuleLibrary.ModuleSize;
    public int Height => MacroRows * ModuleLibrary.ModuleSize;

    public MacroRoomTemplate(string name, RoomType type, string[][] macroLayers)
    {
        Name = name;
        Type = type;
        MacroLayers = macroLayers;

        if (macroLayers.Length > 0 && macroLayers[0].Length > 0)
        {
            string sampleRow = macroLayers[0][0];
            MacroCols = sampleRow.Contains(' ') 
                ? sampleRow.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length 
                : sampleRow.Length;
        }
        else
        {
            MacroCols = 0;
        }
    }

    public char GetModuleCode(int layer, int macroRow, int macroCol)
    {
        if (layer >= MacroLayers.Length || macroRow >= MacroLayers[layer].Length)
            return ' ';

        string rowStr = MacroLayers[layer][macroRow];
        if (rowStr.Contains(' '))
        {
            string[] tokens = rowStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return macroCol < tokens.Length ? tokens[macroCol][0] : ' ';
        }

        return macroCol < rowStr.Length ? rowStr[macroCol] : ' ';
    }
}
