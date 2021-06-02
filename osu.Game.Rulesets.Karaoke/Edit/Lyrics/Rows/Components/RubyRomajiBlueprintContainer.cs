﻿// Copyright (c) andy840119 <andy840119@gmail.com>. Licensed under the GPL Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Karaoke.Edit.Lyrics.Components;
using osu.Game.Rulesets.Karaoke.Edit.Lyrics.Rows.Blueprints;
using osu.Game.Rulesets.Karaoke.Objects;
using osu.Game.Rulesets.Karaoke.Objects.Types;
using osu.Game.Rulesets.Karaoke.Utils;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Karaoke.Edit.Lyrics.Rows.Components
{
    public class RubyRomajiBlueprintContainer : ExtendBlueprintContainer<ITextTag>
    {
        [Resolved]
        private ILyricEditorState state { get; set; }

        [Resolved]
        private EditorLyricPiece editorLyricPiece { get; set; }

        [UsedImplicitly]
        private readonly Bindable<RubyTag[]> rubyTags;

        [UsedImplicitly]
        private readonly Bindable<RomajiTag[]> romajiTags;

        protected readonly Lyric Lyric;

        public RubyRomajiBlueprintContainer(Lyric lyric)
        {
            Lyric = lyric;
            rubyTags = lyric.RubyTagsBindable.GetBoundCopy();
            romajiTags = lyric.RomajiTagsBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SelectedItems.BindTo(state.SelectedTextTags);

            // Add ruby and romaji tag into blueprint container
            RegistBindable(rubyTags);
            RegistBindable(romajiTags);
        }

        protected override bool ApplySnapResult(SelectionBlueprint<ITextTag>[] blueprints, SnapResult result)
        {
            if (!base.ApplySnapResult(blueprints, result))
                return false;

            // handle lots of ruby / romaji drag position changed.
            var items = blueprints.Select(x => x.Item).ToArray();
            if (!items.Any())
                return false;

            var leftPosition = ToLocalSpace(result.ScreenSpacePosition).X;
            var startIndex = TextIndexUtils.ToStringIndex(editorLyricPiece.GetHoverIndex(leftPosition));
            var diff = startIndex - items.First().StartIndex;
            if (diff == 0)
                return false;

            foreach (var item in items)
            {
                var newStartIndex = item.StartIndex + diff;
                var newEndIndex = item.EndIndex + diff;
                if (!LyricUtils.AbleToInsertTextTagAtIndex(Lyric, newStartIndex) || !LyricUtils.AbleToInsertTextTagAtIndex(Lyric, newEndIndex))
                    continue;

                item.StartIndex = newStartIndex;
                item.EndIndex = newEndIndex;
            }

            return true;
        }

        protected override IEnumerable<SelectionBlueprint<ITextTag>> SortForMovement(IReadOnlyList<SelectionBlueprint<ITextTag>> blueprints)
            => blueprints.OrderBy(b => b.Item.StartIndex);

        protected override SelectionHandler<ITextTag> CreateSelectionHandler()
            => new RubyRomajiSelectionHandler();

        protected override SelectionBlueprint<ITextTag> CreateBlueprintFor(ITextTag item)
        {
            switch (item)
            {
                case RubyTag rubyTag:
                    return new RubyTagSelectionBlueprint(rubyTag);

                case RomajiTag romajiTag:
                    return new RomajiTagSelectionBlueprint(romajiTag);

                default:
                    throw new ArgumentOutOfRangeException(nameof(item));
            }
        }

        protected override void DeselectAll()
        {
            state.ClearSelectedTextTags();
        }

        protected class RubyRomajiSelectionHandler : ExtendSelectionHandler<ITextTag>
        {
            [Resolved]
            private ILyricEditorState state { get; set; }

            [Resolved]
            private LyricManager lyricManager { get; set; }

            [Resolved]
            private EditorLyricPiece editorLyricPiece { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                SelectedItems.BindTo(state.SelectedTextTags);
            }

            // for now we always allow movement. snapping is provided by the Timeline's "distance" snap implementation
            public override bool HandleMovement(MoveSelectionEvent<ITextTag> moveEvent) => true;

            protected override void DeleteItems(IEnumerable<ITextTag> items)
            {
                // todo : delete ruby or romaji
            }

            private float deltaPosition = 0;

            protected override void OnOperationBegan()
            {
                base.OnOperationBegan();
                deltaPosition = 0;
            }

            public override bool HandleScale(Vector2 scale, Anchor anchor)
            {
                deltaPosition += scale.X;

                // this feature only works if only select one ruby / romaji tag.
                var selectedTextTag = SelectedItems.FirstOrDefault();
                if (selectedTextTag == null)
                    return false;

                // get real left-side and right-side position
                var rect = editorLyricPiece.GetTextTagPosition(selectedTextTag);

                switch (anchor)
                {
                    case Anchor.CentreLeft:
                        var leftPosition = rect.Left + deltaPosition;
                        var startIndex = TextIndexUtils.ToStringIndex(editorLyricPiece.GetHoverIndex(leftPosition));
                        if (startIndex >= selectedTextTag.EndIndex)
                            return false;

                        selectedTextTag.StartIndex = startIndex;
                        return true;

                    case Anchor.CentreRight:
                        var rightPosition = rect.Right + deltaPosition;
                        var endIndex = TextIndexUtils.ToStringIndex(editorLyricPiece.GetHoverIndex(rightPosition));
                        if (endIndex <= selectedTextTag.StartIndex)
                            return false;

                        selectedTextTag.EndIndex = endIndex;
                        return true;

                    default:
                        return false;
                }
            }

            protected override void OnSelectionChanged()
            {
                base.OnSelectionChanged();

                // only select one ruby / romaji tag can let user drag to change start and end index.
                SelectionBox.CanScaleX = SelectedItems.Count == 1;
            }
        }
    }
}
