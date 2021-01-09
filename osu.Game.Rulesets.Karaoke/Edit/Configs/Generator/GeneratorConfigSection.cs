﻿// Copyright (c) andy840119 <andy840119@gmail.com>. Licensed under the GPL Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Karaoke.Edit.Generator.Types;
using osuTK;

namespace osu.Game.Rulesets.Karaoke.Edit.Configs.Generator
{
    public abstract class GeneratorConfigSection<TConfig> : GeneratorConfigSection where TConfig : IHasConfig<TConfig>, new()
    {
        private TConfig defaultConfig;
        private Bindable<TConfig> current;

        protected GeneratorConfigSection(Bindable<TConfig> current)
        {
            this.current = current;
            defaultConfig = new TConfig().CreateDefaultConfig();
        }

        protected void RegistConfig<TValue>(Bindable<TValue> bindable, string propertyName)
        {
            // set default value
            var defaultValue = getConfigValue<TValue>(defaultConfig, propertyName);
            bindable.Default = defaultValue;

            // set current value
            current.BindValueChanged(e =>
            {
                var currentValue = getConfigValue<TValue>(e.NewValue, propertyName);
                bindable.Value = currentValue;
            });

            // save value if control changed
            bindable.BindValueChanged(e =>
            {
                setConfigValue(propertyName, e.NewValue);
                current.TriggerChange();
            });
        }

        private TValue getConfigValue<TValue>(TConfig config, string propertyName)
            => (TValue)config.GetType().GetProperty(propertyName).GetValue(config);

        private void setConfigValue(string propertyName, object value)
            => current.Value.GetType().GetProperty(propertyName).SetValue(current.Value, value);

        internal enum ApplyConfigFrom
        {
            Default,

            Current,
        }
    }

    public abstract class GeneratorConfigSection : Container
    {
        private readonly FillFlowContainer flow;

        [Resolved]
        protected OsuColour Colours { get; private set; }

        [Resolved]
        protected IBindable<WorkingBeatmap> Beatmap { get; private set; }

        protected override Container<Drawable> Content => flow;

        protected abstract string Title { get; }

        protected GeneratorConfigSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 18),
                    Text = Title,
                },
                flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Top = 30 }
                }
            };
        }
    }
}
