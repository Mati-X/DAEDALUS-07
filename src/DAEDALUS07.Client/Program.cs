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
        var font8x8 = host.LoadFont("Fonts/IBM8x8.font");
        Theme.Load("Themes/default.json");
        GameHost.Instance.DefaultFont = customFont;

        int padding = 100;
        SubnetGrid grid = new(280 + padding * 2, 180 + padding * 2);
        var (rooms, spawn, enemies) = BspDungeonGenerator.Generate(grid, minSize: 65, maxSplits: 3, new Random(), padding);

        Entity player = new(spawn.x, spawn.y, "THESEUS")
            { Size = 2 };

    ;                                                                                                                                                                        
                                                                                                                                                                                                                               
        ScreenObject root = new ScreenObject();                                                                                                                                                                                  
                                                                                                                                                                                                                                
        SubnetRenderer renderer = new(grid, player,enemies, rooms, customFont);                                                                                                                                                                             
        renderer.Position = new Point(0, 0);                                                                                                                                                                                     
        renderer.Font = font8x8;                                                                                                                                                                                              
        root.Children.Add(renderer);                                                                                                                                                                                             
        
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
                                                                                                                                                                                                                                    
        graphics.ApplyChanges();    
        
        Game.Instance.Screen = root;                                                                                                                                                                                             
    }        