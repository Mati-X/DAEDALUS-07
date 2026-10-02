 using DAEDALUS07.Client.Rendering;
 using DAEDALUS07.Core.Entities;
 using DAEDALUS07.Core.Generation;
 using DAEDALUS07.Core.Grid;
 using SadConsole;                                                                                                                                                                                                                            
 using SadConsole.Configuration;
 using SadRogue.Primitives;

 Settings.WindowTitle = "DAEDALUS-07 // SYSTEM INFILTRATION";
 Settings.AllowWindowResize = true;
 Settings.ResizeMode = Settings.WindowResizeOptions.Fit;                                                                                                                                                                     
                                                                                                                                                                                                                                                 
    Builder configuration = new Builder()                                                                                                                                                                                                        
        .SetWindowSizeInCells(160, 45)                                                                                                                                                                                                           
        .OnStart(Startup);                                                                                                                                                                                                                       
                                                                                                                                                                                                                                                 
    Game.Create(configuration);                                                                                                                                                                                                                  
    Game.Instance.Run();                                                                                                                                                                                                                         
    Game.Instance.Dispose();                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                 
     void Startup(object? sender, GameHost host)                                                                                                                                                                                  
    {                                                                                                                                                                                                                            
        var customFont = host.LoadFont("Fonts/IBM16x16.font");                                                                                                                                                                     
        GameHost.Instance.DefaultFont = customFont;                                                                                                                                                                              
                                                                                                                                                                                                                                 
        int padding = 100;                                                                                                                                                                                                       
        SubnetGrid grid = new(140 + padding * 2, 90 + padding * 2);                                                                                                                                                              
        var (rooms, spawn) = BspDungeonGenerator.Generate(grid, minSize: 16, maxSplits: 4, new Random(), padding);                                                                                                               
                                                                                                                                                                                                                                 
        Entity player = new(spawn.x, spawn.y, "THESEUS");                                                                                                                                                                        
                                                                                                                                                                                                                               
        ScreenObject root = new ScreenObject();                                                                                                                                                                                  
                                                                                                                                                                                                                                
        SubnetRenderer renderer = new(grid, player);                                                                                                                                                                             
        renderer.Position = new Point(0, 0);                                                                                                                                                                                     
        renderer.Font = customFont;                                                                                                                                                                                              
        root.Children.Add(renderer);                                                                                                                                                                                             
                                                                                                                                                                                                                                
        SadConsole.Console messageLog = new SadConsole.Console(56, 9);                                                                                                                                                                                
        messageLog.Position = new Point(0, 36);                                                                                                                                                                                  
        messageLog.Font = customFont;                                                                                                                                                                                             
        messageLog.Surface.DrawBox(new Rectangle(0, 0, 56, 9), ShapeParameters.CreateStyledBox(                                                                                                                                                                              
            ICellSurface.ConnectedLineThin,                                                                                                                                                                                          
            new ColoredGlyph(Color.DarkSlateGray, Color.Black)                                                                                                                                                                       
        ));                                                                                      
        messageLog.Surface.Print(2, 0, " [TERMINAL // MINOS_NET] ", Color.Cyan);                                                                                                                                                
        root.Children.Add(messageLog);                                                                                                                                                                                           
        
        SadConsole.Console cyberdeckHud = new SadConsole.Console(23, 45);                                                                                                                                                                              
        cyberdeckHud.Position = new Point(57, 0);                                                                                                                                                                                
        cyberdeckHud.Font = customFont;                                                                                                                                                                                         
        cyberdeckHud.Surface.DrawBox(new Rectangle(0, 0, 23, 45), ShapeParameters.CreateStyledBox(                                                                                                                                                                              
            ICellSurface.ConnectedLineThin,                                                                                                                                                                                          
            new ColoredGlyph(Color.DarkSlateGray, Color.Black)                                                                                                                                                                       
        ));                                                                                    
        cyberdeckHud.Surface.Print(2, 0, " [CYBERDECK] ", Color.Yellow);                                                                                                                                                         
        root.Children.Add(cyberdeckHud);             
        
        var graphics = (Microsoft.Xna.Framework.GraphicsDeviceManager)                                                                                                                                                               
            SadConsole.Game.Instance.MonoGameInstance.Services.GetService(typeof(Microsoft.Xna.Framework.IGraphicsDeviceManager))!;                                                                                                   
                                                                                                                                                                                                                                 
        graphics.IsFullScreen = true;                                                                                                                                                                                                
        graphics.ApplyChanges();    
        
        Game.Instance.Screen = root;                                                                                                                                                                                             
    }        