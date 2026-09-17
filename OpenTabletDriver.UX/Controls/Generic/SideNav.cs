using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public sealed class SideNav : Panel
    {
        public SideNav()
        {
            _list = new StackLayout
            {
                Padding = new Padding(8),
                Spacing = 2,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            var navPanel = new Panel
            {
                BackgroundColor = SystemColors.ControlBackground,
                Width = AppStyle.NavWidth,
                Content = _list
            };

            _host = new Panel
            {
                Padding = AppStyle.PagePadding
            };

            var splitter = new Splitter
            {
                Orientation = Orientation.Horizontal,
                Panel1MinimumSize = 160,
                Panel2MinimumSize = 320,
                FixedPanel = SplitterFixedPanel.Panel1,
                Position = AppStyle.NavWidth,
                Panel1 = new Scrollable
                {
                    Border = BorderType.None,
                    Content = navPanel
                },
                Panel2 = _host
            };

            Content = splitter;
        }

        private readonly StackLayout _list;
        private readonly Panel _host;
        private readonly List<Entry> _entries = [];
        private Entry? _selected;

        public Control? SelectedContent => _selected?.Content;

        public event EventHandler<EventArgs>? SelectedContentChanged;

        public void Add(string text, Control content, bool visible = true)
        {
            Insert(_entries.Count, text, content, visible);
        }

        public void InsertBefore(Control before, string text, Control content, bool visible = true)
        {
            var index = _entries.FindIndex(e => ReferenceEquals(e.Content, before));
            if (index < 0)
                index = _entries.Count;
            Insert(index, text, content, visible);
        }

        public void SetVisible(Control content, bool visible)
        {
            var entry = Find(content);
            if (entry == null)
                return;

            entry.Visible = visible;
            entry.Row.Visible = visible;

            if (!visible && ReferenceEquals(_selected, entry))
                SelectFirstVisible();
            else if (visible && _selected == null)
                Select(entry);
        }

        public void Select(Control content)
        {
            var entry = Find(content);
            if (entry == null || !entry.Visible)
                return;
            Select(entry);
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            if (Content is Splitter splitter)
                splitter.Position = AppStyle.NavWidth;
        }

        private void Insert(int index, string text, Control content, bool visible)
        {
            var row = new NavRow(text);
            var entry = new Entry(text, content, row) { Visible = visible };
            row.MouseDown += (_, _) =>
            {
                if (entry.Visible)
                    Select(entry);
            };
            row.Visible = visible;

            _entries.Insert(index, entry);
            _list.Items.Insert(index, new StackLayoutItem(row, HorizontalAlignment.Stretch));

            if (_selected == null && visible)
                Select(entry);
        }

        private void Select(Entry entry)
        {
            if (ReferenceEquals(_selected, entry))
                return;

            if (_selected != null)
                _selected.Row.SetSelected(false);

            _selected = entry;
            entry.Row.SetSelected(true);
            _host.Content = entry.Content;
            SelectedContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SelectFirstVisible()
        {
            var next = _entries.FirstOrDefault(e => e.Visible);
            if (next != null)
                Select(next);
            else
            {
                _selected = null;
                _host.Content = null;
            }
        }

        private Entry? Find(Control content) =>
            _entries.FirstOrDefault(e => ReferenceEquals(e.Content, content));

        private sealed class Entry
        {
            public Entry(string text, Control content, NavRow row)
            {
                Text = text;
                Content = content;
                Row = row;
            }

            public string Text { get; }
            public Control Content { get; }
            public NavRow Row { get; }
            public bool Visible { get; set; } = true;
        }

        private sealed class NavRow : Panel
        {
            public NavRow(string text)
            {
                _label = new Label
                {
                    Text = text,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Padding = new Padding(12, 8);
                Content = _label;
                Cursor = new Cursor(CursorType.Pointer);
            }

            private readonly Label _label;
            private bool _selected;

            public void SetSelected(bool selected)
            {
                _selected = selected;
                BackgroundColor = selected ? SystemColors.Highlight : Colors.Transparent;
                _label.TextColor = selected ? SystemColors.HighlightText : SystemColors.ControlText;
            }

            protected override void OnMouseEnter(MouseEventArgs e)
            {
                if (!_selected)
                    BackgroundColor = SystemColors.ControlBackground;
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(MouseEventArgs e)
            {
                if (!_selected)
                    BackgroundColor = Colors.Transparent;
                base.OnMouseLeave(e);
            }
        }
    }
}
