using fluXis.Game.Graphics.Background;
using fluXis.Game.Input;
using fluXis.Resources;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;

namespace fluXis.Game
{
    public class FluXisGameBase : osu.Framework.Game
    {
        // Anything in this class is shared between the test browser and the game implementation.
        // It allows for caching global dependencies that should be accessible to tests, or changing
        // the screen scaling for all components including the test browser and framework overlays.

        private DependencyContainer dependencies;

        protected override Container<Drawable> Content => content;
        private Container content;

        protected FluXisGameBase()
        {
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            Resources.AddStore(new DllResourceStore(FluXisResources.ResourceAssembly));
            InitFonts();

            dependencies.Cache(new BackgroundTextureStore(Host, storage));

            FluXisKeybindContainer keybinds;

            base.Content.Add(new SafeAreaContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawSizePreservingFillContainer
                {
                    TargetDrawSize = new Vector2(1920, 1080),
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        keybinds = new FluXisKeybindContainer(this)
                    }
                }
            });

            dependencies.Cache(keybinds);
        }

        protected void InitFonts()
        {
            AddFont(Resources, @"Fonts/Quicksand/Quicksand");
            AddFont(Resources, @"Fonts/Quicksand/Quicksand-SemiBold");
            AddFont(Resources, @"Fonts/Quicksand/Quicksand-Bold");
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
    }
}
