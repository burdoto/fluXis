﻿using fluXis.Game.Map;
using fluXis.Game.Screens.Gameplay.Ruleset;
using fluXis.Game.Audio;
using fluXis.Game.Input;
using fluXis.Game.Integration;
using fluXis.Game.Scoring;
using fluXis.Game.Screens.Gameplay.HUD;
using fluXis.Game.Screens.Gameplay.HUD.Judgement;
using fluXis.Game.Screens.Gameplay.Input;
using fluXis.Game.Screens.Gameplay.UI;
using fluXis.Game.Screens.Result;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;

namespace fluXis.Game.Screens.Gameplay
{
    public class GameplayScreen : Screen, IKeyBindingHandler<FluXisKeybind>
    {
        private bool starting = true;
        private bool ended;
        private bool restarting;
        private bool paused;

        public GameplayInput Input;
        public Performance Performance;
        public MapInfo Map;
        public JudgementDisplay JudgementDisplay;
        public Playfield Playfield { get; private set; }

        public Sample HitSound;
        public Sample Combobreak;
        public Sample Restart;

        public GameplayScreen(MapInfo map)
        {
            Map = map;
        }

        [BackgroundDependencyLoader]
        private void load(ISampleStore samples)
        {
            Input = new GameplayInput(Map.KeyCount);
            Performance = new Performance(Map);

            HitSound = samples.Get("Gameplay/hitsound.mp3");
            Combobreak = samples.Get("Gameplay/combobreak.ogg");
            Restart = samples.Get("Gameplay/restart.ogg");

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                Input,
                Playfield = new Playfield(this),
                new PauseMenu(this)
            };

            AddInternal(new ComboCounter(this));
            AddInternal(new AccuracyDisplay(this));
            AddInternal(new Progressbar(this));
            AddInternal(JudgementDisplay = new JudgementDisplay(this));
            AddInternal(new AutoPlayDisplay(this));
            AddInternal(new JudgementCounter(Performance));

            Discord.Update("Playing a map", $"{Map.Metadata.Title} - {Map.Metadata.Artist} [{Map.Metadata.Difficulty}]", "playing", 0, (int)((Map.EndTime - Conductor.CurrentTime) / 1000));
        }

        protected override void LoadComplete()
        {
            Conductor.PlayTrack(Map);
            Conductor.CurrentTime = -2000;

            base.LoadComplete();
        }

        protected override void Update()
        {
            if (Conductor.CurrentTime >= 0 && starting)
            {
                starting = false;
                Conductor.ResumeTrack();
            }

            if (!starting && Playfield.Manager.IsFinished)
            {
                End();
            }

            base.Update();
        }

        public void Die()
        {
            Conductor.SetSpeed(.25f, 1000, Easing.None, true);
        }

        public void End()
        {
            if (ended)
                return;

            ended = true;

            this.Push(new ResultsScreen(Map, Performance));
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            fadeOut();
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            Alpha = 0;
            this.ScaleTo(1.2f)
                .ScaleTo(1f, 750, Easing.OutQuint)
                .Delay(250)
                .FadeIn(250);

            base.OnEntering(e);
        }

        private void fadeOut()
        {
            this.FadeOut(restarting ? 0 : 250);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            // probably coming back from results screen
            // so just go to song select
            this.Exit();

            base.OnResuming(e);
        }

        public bool OnPressed(KeyBindingPressEvent<FluXisKeybind> e)
        {
            switch (e.Action)
            {
                case FluXisKeybind.ToggleAutoplay:
                    Playfield.Manager.AutoPlay.Value = !Playfield.Manager.AutoPlay.Value;
                    return true;

                case FluXisKeybind.ForceDeath:
                    Die();
                    return true;

                case FluXisKeybind.Restart:
                    restarting = true;
                    Restart.Play();
                    this.Push(new GameplayScreen(Map)); // TODO: restart in a better way
                    return true;

                case FluXisKeybind.Pause:
                    Conductor.SetSpeed(paused ? 1 : 0);
                    paused = !paused;
                    return true;

                case FluXisKeybind.Skip:
                    if (Map.StartTime - Conductor.CurrentTime > 2000)
                        Conductor.Seek(Map.StartTime - 2000);
                    return true;

                case FluXisKeybind.ForceEnd:
                    End();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<FluXisKeybind> e)
        {
            // nothing to do here...
        }
    }
}
