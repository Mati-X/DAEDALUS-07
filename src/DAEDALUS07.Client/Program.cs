 using DAEDALUS07.Client.Rendering;
 using DAEDALUS07.Core.Entities;
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
        SubnetGrid grid = new(10, 10);   
        Entity player = new(6, 6, "THESEUS");       
        grid.CreateRoom(5,5,5,5);
        SubnetRenderer renderer = new(grid, player);                                                                                                                                                                                                  
        Game.Instance.Screen = renderer;                                                                                                                                                                                                           
    }   