﻿// Copyright (c) andy840119 <andy840119@gmail.com>. Licensed under the GPL Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Karaoke.Edit.LyricEditor.Components;
using osu.Game.Rulesets.Karaoke.Edit.LyricEditor.Components.Badges;
using osu.Game.Rulesets.Karaoke.Objects;
using osuTK;

namespace osu.Game.Rulesets.Karaoke.Edit.LyricEditor
{
    public class LyricRearrangeableListContainer : OsuRearrangeableListContainer<LyricLine>
    {
        protected override OsuRearrangeableListItem<LyricLine> CreateOsuDrawable(LyricLine item)
            => new LyricRearrangeableListItem(item);

        public class LyricRearrangeableListItem : OsuRearrangeableListItem<LyricLine>
        {
            private Box background;
            private Box headerBackground;

            public LyricRearrangeableListItem(LyricLine item)
                    : base(item)
            {
            }

            protected override Drawable CreateContent()
            {
                return new Container
                {
                    Masking = true,
                    CornerRadius = 5,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.3f
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Content = new[]
                            {
                                new[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            headerBackground = new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Alpha = 0.7f
                                            },
                                            new BadgeFillFlowContainer
                                            {
                                                Direction = FillDirection.Vertical,
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.TopRight,
                                                Origin = Anchor.TopRight,
                                                Spacing = new Vector2(5),
                                                Padding = new MarginPadding(10),
                                                Children = new Badge[]
                                                {
                                                    new TimeInfoBadge(Model),
                                                    new StyleInfoBadge(Model),
                                                    new LayoutInfoBadge(Model),
                                                }
                                            },
                                        }
                                    },
                                    new LyricControl(Model)
                                    {
                                        Margin = new MarginPadding{ Left = 10},
                                        RelativeSizeAxes = Axes.X,
                                    }
                                }
                            },
                            ColumnDimensions = new[] { new Dimension(GridSizeMode.Absolute, 200) },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Gray7;
                headerBackground.Colour = colours.Gray2;
            }

            public class BadgeFillFlowContainer : FillFlowContainer<Badge>
            {
                public override void Add(Badge drawable)
                {
                    drawable.Anchor = Anchor.TopRight;
                    drawable.Origin = Anchor.TopRight;
                    base.Add(drawable);
                }
            }
        }
    }
}
