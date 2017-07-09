using System;
using Engine;
using Engine.Animation;
using Engine.Interfaces;

namespace {{{GameName}}}
{
    public class LandingAreaLayout : ILayoutView
    {
        private readonly IRenderer renderer;
        private readonly Game game;
        private ILayer mainLayer;
        private MotionManager helloWorldAnimation;
        private MotionManager welcomeAnimation;
        public ILayout Layout { get; set; }
        public ITouchManager TouchManager { get; private set; }
        public LandingAreaLayoutPositions Positions { get; set; }
        public LandingAreaState State { get; set; }

        public LandingAreaLayout(Game game, IRenderer renderer, ILayout layout)
        {
            Positions = new LandingAreaLayoutPositions(layout);
            this.game = game;
            this.renderer = renderer;
            Layout = layout;
            State = new LandingAreaState();
        }


        public void Render(TimeSpan elapsedGameTime)
        {
            mainLayer.Begin();

            mainLayer.Save();
            mainLayer.Clear();

            if (helloWorldAnimation != null) helloWorldAnimation.Render(mainLayer);
            if (State.ShowWelcome)
            {
                if (welcomeAnimation != null) welcomeAnimation.Render(mainLayer);
            }

            mainLayer.Restore();

            mainLayer.End();
        }

        public void Destroy()
        {
        }

        public void InitLayoutView()
        {
            mainLayer = renderer.CreateLayer(Layout.Width, Layout.Height, Layout);
            renderer.AddLayer(mainLayer);

            TouchManager = new TouchManager(game.Client);
            TouchManager.PushClickRect(Positions.HelloWorldPosition, Assets.Images.Landing.HelloWorld, (type, box, x, y, collide) => {
                if (type == TouchType.TouchDown)
                {
                    if (!State.ShowWelcome)
                    {
                        State.ShowWelcome = true;
                    }
                }
                return true;
            }, true);

            helloWorldAnimation = MotionManager
                .StartMotion(Positions.HelloWorldPosition.X, 0)
                .Motion(new AnimationMotion(Positions.HelloWorldPosition, 1600, AnimationEasing.SineEaseIn))
                .Motion(new AnimationMotion(Positions.HelloWorldPosition.X, Layout.Height / 4, 800, AnimationEasing.SineEaseIn))
                .Motion(new AnimationMotion(Positions.HelloWorldPosition, 600, AnimationEasing.SineEaseIn))
                .Motion(new FinishMotion())
                .OnRender((layer, posX, posY, animationIndex, percentDone) => {
                    layer.Save();
                    layer.DrawImage(Assets.Images.Landing.HelloWorld, posX, posY, true);
                    layer.Restore();
                });

            welcomeAnimation = MotionManager
                .StartMotion(Positions.WelcomePosition.X, 0)
                .Motion(new AnimationMotion(Positions.WelcomePosition, 1600, AnimationEasing.SineEaseIn))
                .Motion(new AnimationMotion(Positions.WelcomePosition.X, Layout.Height / 2, 800, AnimationEasing.SineEaseIn))
                .Motion(new AnimationMotion(Positions.WelcomePosition, 600, AnimationEasing.SineEaseIn))
                .Motion(new FinishMotion())
                .OnRender((layer, posX, posY, animationIndex, percentDone) => {
                    layer.Save();
                    layer.DrawImage(Assets.Images.Landing.Welcome, posX, posY, true);
                    layer.Restore();
                });

        }

        public void TickLayoutView(TimeSpan elapsedGameTime)
        {
            if (!helloWorldAnimation.Completed)
            {
                helloWorldAnimation.Tick(elapsedGameTime);
            }

            if (State.ShowWelcome)
            {
                if (!welcomeAnimation.Completed)
                {
                    welcomeAnimation.Tick(elapsedGameTime);
                }
            }
        }

    }
    public class LandingAreaState
    {
        public bool ShowWelcome { get; set; }
    }


    public class LandingAreaLayoutPositions
    {
        public Point WelcomePosition { get; set; }
        public Point HelloWorldPosition { get; set; }

        public LandingAreaLayoutPositions(ILayout layout)
        {
            HelloWorldPosition = new Point(layout.Width / 2, layout.Height / 2);
            WelcomePosition = new Point(layout.Width / 2, layout.Height / 4 * 3);
        }

    }
}