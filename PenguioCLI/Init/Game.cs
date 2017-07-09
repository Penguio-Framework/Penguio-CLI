using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Interfaces;

namespace {{{GameName}}}
{
    public class Game : IGame
    {
        public IScreen landingScreen;

        public GameService GameService { get; set; }

        public ILayout LandingAreaLayout { get; set; }

        public IScreenManager ScreenManager { get; set; }
        public IClient Client { get; set; }
        public AssetManager AssetManager { get; set; }



        public void InitScreens(IRenderer renderer, IScreenManager screenManager)
        {
            ScreenManager = screenManager;

            var screenTransitioner = new ScreenTransitioner(this);
            int width = 1536;
            int height = 2048;

            landingScreen = screenManager.CreateScreen();
            LandingAreaLayout = landingScreen.CreateLayout(width, height).MakeActive().SetScreenOrientation(ScreenOrientation.Vertical);
            LandingAreaLayout.LayoutView = new LandingAreaLayout(this, GameService, renderer, LandingAreaLayout);

            ScreenManager.ChangeScreen(landingScreen);
        }


        public void InitSocketManager(ISocketManager socketManager)
        {
          
        }


        public void BeforeDraw()
        {
        }

        public void AfterDraw()
        {
        }


        public void BeforeTick()
        {
        }

        public void AfterTick()
        {
        }


        public void LoadAssets(IRenderer renderer)
        { 
           Assets.LoadAssets(renderer, AssetManager);
        }
    }
}
