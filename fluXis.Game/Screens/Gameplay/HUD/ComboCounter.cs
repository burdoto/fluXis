using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace fluXis.Game.Screens.Gameplay.HUD
{
    public class ComboCounter : GameplayHUDElement
    {
        public ComboCounter(GameplayScreen screen)
            : base(screen)
        {
        }

        private SpriteText text;
        private int lastCombo;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.BottomCentre;
            Height = 64;

            Add(text = new SpriteText
            {
                Font = new FontUsage("Quicksand", 64f, "SemiBold", false, true),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0
            });
        }

        protected override void Update()
        {
            if (Screen.Performance.Combo < lastCombo)
                text.FadeColour(Colour4.Red).FadeColour(Colour4.White, 200).FadeOut(200).ScaleTo(1.4f, 300);

            if (Screen.Performance.Combo > lastCombo && Screen.Performance.Combo >= 5)
            {
                text.Text = Screen.Performance.Combo.ToString();
                text.FadeColour(Colour4.White).FadeIn(400);
                text.ScaleTo(1.05f).ScaleTo(1f, 100);
            }

            lastCombo = Screen.Performance.Combo;
            base.Update();
        }
    }
}
