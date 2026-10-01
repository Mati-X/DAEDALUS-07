 using DAEDALUS07.Client.Rendering;
 using DAEDALUS07.Core.Entities;
 using DAEDALUS07.Core.Generation;
 using DAEDALUS07.Core.Grid;
 using SadConsole;                                                                                                                                                                                                                            
    using SadConsole.Configuration;                                                                                                                                                                                                              
                                                                                                                                                                                                                                                 
    Settings.WindowTitle = "DAEDALUS-07 // SYSTEM INFILTRATION";                                                                                                                                                                                 
                                                                                                                                                                                                                                                 
    Builder configuration = new Builder()                                                                                                                                                                                                        
        .SetWindowSizeInCells(100, 35)                                                                                                                                                                                                           
        .OnStart(Startup);                                                                                                                                                                                                                       
                                                                                                                                                                                                                                                 
    Game.Create(configuration);                                                                                                                                                                                                                  
    Game.Instance.Run();                                                                                                                                                                                                                         
    Game.Instance.Dispose();                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                 
    void Startup(object? sender, GameHost host)
    {
         int padding = 100;
         SubnetGrid grid = new(140 + padding*2, 90 + padding*2);                                                                                                                                                                                                               
                                                                                                                                                                                                                                                
    var (rooms, spawn) = BspDungeonGenerator.Generate(grid, minSize: 16, maxSplits: 4, new Random(), padding);                                                                                                                                             
                                                                                                                                                                                                                                                
    Entity player = new(spawn.x, spawn.y, "THESEUS");                                                                                                                                                                                            
                                                                                                                                                                                                                                                 
    SubnetRenderer renderer = new(grid, player);                                                                                                                                                                                                 
    Game.Instance.Screen = renderer;                                                                                                                                                                                                              
    }   