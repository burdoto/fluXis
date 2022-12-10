using System.Collections.Generic;
using System.Linq;
using fluXis.Game.Audio;
using fluXis.Game.Map;
using fluXis.Game.Scoring;
using fluXis.Game.Screens.Gameplay.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace fluXis.Game.Screens.Gameplay.Ruleset
{
    public class HitObjectManager : CompositeDrawable
    {
        public Playfield Playfield { get; }
        public Performance Performance { get; }
        public List<HitObject> FutureHitObjects { get; } = new List<HitObject>();
        public List<HitObject> HitObjects { get; } = new List<HitObject>();

        public bool IsFinished => FutureHitObjects.Count == 0 && HitObjects.Count == 0;

        private const bool auto_play = false;

        public HitObjectManager(Playfield playfield)
        {
            Playfield = playfield;
            Performance = playfield.Screen.Performance;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1, 1);
        }

        protected override void Update()
        {
            if (auto_play)
                updateAutoPlay();
            else
                updateInput();

            while (FutureHitObjects != null && FutureHitObjects.Count > 0 && FutureHitObjects[0].Data.Time <= Conductor.Time + 2000 * HitObject.ScrollSpeed)
            {
                HitObject hitObject = FutureHitObjects[0];
                FutureHitObjects.RemoveAt(0);
                HitObjects.Add(hitObject);
                AddInternal(hitObject);
            }

            foreach (var internalChild in InternalChildren)
            {
                HitObject hitObject = (HitObject)internalChild;

                if (hitObject.Missed && hitObject.Exists)
                {
                    miss(hitObject);
                }
            }

            base.Update();
        }

        private void updateAutoPlay()
        {
            List<HitObject> belowTime = HitObjects.Where(h => h.Data.Time <= Conductor.Time && h.Exists).ToList();

            foreach (var hitObject in belowTime.Where(h => !h.GotHit).ToList())
            {
                Playfield.Screen.HitSound.Play();
                hit(hitObject, false);
            }

            foreach (var hitObject in belowTime.Where(h => h.Data.IsLongNote()).ToList())
            {
                hitObject.IsBeingHeld = true;

                if (hitObject.Data.HoldEndTime <= Conductor.Time)
                {
                    Playfield.Screen.HitSound.Play();
                    hit(hitObject, true);
                }
            }
        }

        private void updateInput()
        {
            GameplayInput input = Playfield.Screen.Input;

            if (input.JustPressed.Contains(true))
            {
                Playfield.Screen.HitSound.Play();

                List<HitObject> hitable = new List<HitObject>();

                foreach (var hit in HitObjects)
                {
                    if (hit.Hitable && input.JustPressed[hit.Data.Lane - 1])
                        hitable.Add(hit);
                }

                bool[] pressed = { false, false, false, false };

                if (hitable.Count > 0)
                {
                    foreach (var hitObject in hitable)
                    {
                        if (!pressed[hitObject.Data.Lane - 1])
                        {
                            hit(hitObject, false);
                            pressed[hitObject.Data.Lane - 1] = true;
                        }
                    }
                }
            }

            if (input.Pressed.Contains(true))
            {
                foreach (var hit in HitObjects)
                {
                    if (hit.Hitable && hit.GotHit && hit.Data.IsLongNote() && input.Pressed[hit.Data.Lane - 1])
                        hit.IsBeingHeld = true;
                }
            }

            if (input.JustReleased.Contains(true))
            {
                List<HitObject> releaseable = new List<HitObject>();

                foreach (var hit in HitObjects)
                {
                    if (hit.Releasable && input.JustReleased[hit.Data.Lane - 1])
                        releaseable.Add(hit);
                }

                bool[] pressed = { false, false, false, false };

                if (releaseable.Count > 0)
                {
                    foreach (var hitObject in releaseable)
                    {
                        if (!pressed[hitObject.Data.Lane - 1])
                        {
                            hit(hitObject, true);
                            pressed[hitObject.Data.Lane - 1] = true;
                        }
                    }
                }
            }
        }

        private void hit(HitObject hitObject, bool isHoldEnd)
        {
            int diff = isHoldEnd ? hitObject.Data.HoldEndTime - Conductor.Time : hitObject.Data.Time - Conductor.Time;
            hitObject.GotHit = true;
            judmentDisplay(diff);

            Performance.IncCombo();

            if (hitObject.Data.IsLongNote() && !isHoldEnd) { }
            else
            {
                hitObject.Kill(false);
                HitObjects.Remove(hitObject);
            }
        }

        private void miss(HitObject hitObject)
        {
            if (Performance.Combo >= 5)
                Playfield.Screen.Combobreak.Play();

            judmentDisplay(150);
            Performance.ResetCombo();

            hitObject.Kill(true);
            HitObjects.Remove(hitObject);
        }

        private void judmentDisplay(int diff)
        {
            Judgement judgement = Judgement.FromTiming(diff);
            Performance.AddJudgement(judgement.Key);
            Playfield.Screen.JudgementDisplay.PopUp(judgement);
        }

        public void LoadMap(MapInfo map)
        {
            map.Sort();

            foreach (var hit in map.HitObjects)
            {
                HitObject hitObject = new HitObject(this, hit);
                FutureHitObjects.Add(hitObject);
            }
        }
    }
}