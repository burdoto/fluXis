using fluXis.Game.Map;
using fluXis.Game.Scoring;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace fluXis.Game.Screens.Result.UI
{
    public class ResultHitPoints : Container
    {
        public ResultHitPoints(MapInfo map, Performance performance)
        {
            Height = 300;
            RelativeSizeAxes = Axes.X;

            foreach (var hitPoint in performance.HitPoints)
            {
                AddInternal(new Dot(map, hitPoint));
            }
        }

        private class Dot : CircularContainer
        {
            public Dot(MapInfo map, HitPoint point)
            {
                Size = new Vector2(3);
                Masking = true;
                RelativePositionAxes = Axes.X;
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                X = (point.Time - map.StartTime) / (map.EndTime - map.StartTime);
                Y = point.Difference;

                HitWindow hitWindow = HitWindow.FromKey(point.Judgement);

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = hitWindow.Color
                    }
                };
            }
        }
    }
}
