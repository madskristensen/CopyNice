﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;

namespace CopyNice
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(CopyWithoutIndentation))]
    [ContentType(ContentTypes.Text)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class CopyWithoutIndentation : ICommandHandler<CopyCommandArgs>
    {
        public string DisplayName => nameof(CopyWithoutIndentation);

        [Import]
        private IRtfBuilderService _rtfService { get; set; }

        public bool ExecuteCommand(CopyCommandArgs args, CommandExecutionContext executionContext)
        {
            ITextSelection selection = args.TextView.Selection;

            if (selection.SelectedSpans.Count != 1 // Only handle single selections
                || selection.Start.Position == selection.End.Position // Don't handle zero-width selections
                || selection.Mode == TextSelectionMode.Box // Don't handle box selection
                || !GeneralOptions.Instance.Enabled)
            {
                return false;
            }

            // Only handle selections that starts with indented
            if (args.TextView.TryGetTextViewLineContainingBufferPosition(selection.Start.Position, out ITextViewLine viewLine) && viewLine.Start.Position == selection.Start.Position)
            {
                return false;
            }

            ITextSnapshot snapshot = args.TextView.TextBuffer.CurrentSnapshot;

            IEnumerable<ITextSnapshotLine> lines = from line in snapshot.Lines
                                                   where line.Extent.IntersectsWith(selection.SelectedSpans[0])
                                                   select line;

            // Only handle when multiple lines are selected
            if (lines.Count() == 1)
            {
                return false;
            }

            int indentation = selection.Start.Position.Position - viewLine.Start.Position;
            List<SnapshotSpan> spans = new();
            StringBuilder sb = new();

            string text = args.TextView.TextBuffer.CurrentSnapshot.GetText(viewLine.Start.Position, indentation);

            // Only handle cases when selection starts is on an empty indentation
            if (!string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            foreach (ITextSnapshotLine line in lines)
            {
                if (line.Extent.IsEmpty)
                {
                    spans.Add(line.Extent);
                    sb.AppendLine();
                }
                else
                {
                    int end = Math.Min(line.End.Position, selection.End.Position.Position);
                    SnapshotSpan extent = new(snapshot, Span.FromBounds(line.Start + indentation, end));
                    spans.Add(extent);
                    sb.AppendLine(extent.GetText());
                }
            }

            string rtf = _rtfService.GenerateRtf(new NormalizedSnapshotSpanCollection(spans), args.TextView);

            DataObject data = new();
            data.SetText(rtf.TrimEnd(), TextDataFormat.Rtf);
            data.SetText(sb.ToString().TrimEnd(), TextDataFormat.UnicodeText);
            Clipboard.SetDataObject(data, false);

            return true;
        }

        public CommandState GetCommandState(CopyCommandArgs args)
            => CommandState.Unspecified;
    }
}