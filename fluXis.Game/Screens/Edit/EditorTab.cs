using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Game.Screens.Edit
{
    public class EditorTab : Container
    {
        public Editor Screen { get; set; }

        public EditorTab(Editor screen)
        {
            Screen = screen;
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
        }
    }
}
