﻿using System;
using System.ComponentModel;

namespace Squid
{
    /// <summary>
    /// A single-line text input. Text scrolls horizontally.
    /// </summary>
    [Toolbox]
    public class TextBox : Control, IText
    {
        private float BlinkTime;
        private int DoBlink;
        private string _text = string.Empty;
        private bool IsSelection => SelectStart != SelectEnd;
        private int SelectStart = 0;
        private int SelectEnd = 0;
        private int Offset;
        private int Caret;
        private bool HasFocus;
        private string SavedText;

        /// <summary>
        /// Raised when [text changed].
        /// </summary>
        public event VoidEvent TextChanged;

        /// <summary>
        /// Raised when [text commit].
        /// </summary>
        public event EventHandler TextCommit;

        /// <summary>
        /// Raised when [text cancel].
        /// </summary>
        public event EventHandler TextCancel;

        /// <summary>
        /// Gets or sets a value indicating whether [read only].
        /// </summary>
        /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is password.
        /// </summary>
        /// <value><c>true</c> if this instance is password; otherwise, <c>false</c>.</value>
        public bool IsPassword { get; set; }

        /// <summary>
        /// Gets or sets the password char.
        /// </summary>
        /// <value>The password char.</value>
        public char PasswordChar { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        public TextBoxMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the blink interval.
        /// </summary>
        /// <value>The blink interval.</value>
        [DefaultValue(500f)]
        public float BlinkInterval { get; set; }

        /// <summary>
        /// Gets or sets the color of the selection.
        /// </summary>
        /// <value>The color of the selection.</value>
        [IntColor, DefaultValue(-1)]
        public int SelectionColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>The color of the text.</value>
        [IntColor, DefaultValue(-1)]
        public int TextColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the blink.
        /// </summary>
        /// <value>The color of the blink.</value>
        [IntColor, DefaultValue(-1)]
        public int BlinkColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use text color].
        /// </summary>
        /// <value><c>true</c> if [use text color]; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool UseTextColor { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    _text = value.Replace("\r\n", "");
                }
                else
                {
                    _text = value;
                }

                if (_text == null)
                {
                    _text = string.Empty;
                }

                SelectStart = 0;
                SelectEnd = 0;

                if (TextChanged != null)
                {
                    TextChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets the selection start.
        /// </summary>
        /// <value>The selection start.</value>
        [Xml.XmlIgnore]
        public int SelectionStart => Math.Min(SelectStart, SelectEnd);

        /// <summary>
        /// Gets the selection end.
        /// </summary>
        /// <value>The selection end.</value>
        [Xml.XmlIgnore]
        public int SelectionEnd => Math.Max(SelectStart, SelectEnd);

        /// <summary>
        /// Gets the cursor.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetCursor()
        {
            return Caret;
        }

        /// <summary>
        /// Sets the cursor.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SetCursor(int index)
        {
            Caret = Math.Min(Text.Length, Math.Max(0, index));
            SelectStart = SelectEnd = Caret;
        }

        /// <summary>
        /// Selects the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public void Select(int start, int end)
        {
            SelectStart = Math.Min(Text.Length, Math.Max(0, start));
            SelectEnd = Math.Min(Text.Length, Math.Max(0, end));
            Caret = SelectEnd;
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <value>The selection.</value>
        [Xml.XmlIgnore]
        public string Selection
        {
            get
            {
                if (IsSelection)
                {
                    var masked = Text;
                    if (IsPassword)
                    {
                        masked = new string(PasswordChar, masked.Length);
                    }

                    if (masked == null || masked.Length == 0)
                    {
                        return string.Empty;
                    }

                    var start = Math.Min(SelectStart, SelectEnd);
                    var end = Math.Max(SelectStart, SelectEnd);

                    return masked.Substring(start, end - start);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        public TextBox()
        {
            BlinkColor = -1;
            SelectionColor = -1;
            BlinkInterval = 500;
            PasswordChar = '*';
            Style = "textbox";
            _text = string.Empty;
            AllowFocus = true;
            Cursor = CursorNames.Select;

            MouseDown += TextBox_MouseDown;
            MousePress += TextBox_MousePress;
            MouseDoubleClick += TextBox_MouseDoubleClick;
            LostFocus += TextBox_LostFocus;
            GotFocus += TextBox_GotFocus;
        }

        private void TextBox_GotFocus(Control sender)
        {
            SelectStart = SelectEnd = 0;
        }

        private void TextBox_LostFocus(Control sender)
        {
            if (TextCommit != null)
            {
                TextCommit(this, null);
            }
        }

        private void TextBox_MouseDoubleClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0)
            {
                return;
            }

            var masked = Text;
            if (IsPassword)
            {
                masked = new string(PasswordChar, masked.Length);
            }

            if (string.IsNullOrEmpty(masked))
            {
                return;
            }

            var left = FindIndexLeft(Caret, masked);
            var right = FindIndexRight(Caret, masked);

            if (char.IsWhiteSpace(masked, left) || char.IsPunctuation(masked, left))
            {
                left++;
            }

            if (char.IsWhiteSpace(masked, right - 1) || char.IsPunctuation(masked, right - 1))
            {
                right--;
            }

            SelectStart = left;
            SelectEnd = right;

            Caret = SelectEnd;
        }

        private void TextBox_MousePress(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0)
            {
                return;
            }

            if (Gui.CtrlPressed)
            {
                return;
            }

            var style = Desktop.GetStyle(Style).Styles[State];

            var masked = Text;
            if (IsPassword)
            {
                masked = new string(PasswordChar, masked.Length);
            }

            if (string.IsNullOrEmpty(masked))
            {
                return;
            }

            var font = Gui.Renderer.GetFont(style.Font);
            if (font < 0)
            {
                return;
            }

            var p = Gui.MousePosition - Location;
            var s1 = Gui.Renderer.GetTextSize(masked, font);
            var carex = p.x + Offset + s1.x;
            var x = 0;

            var text = string.Empty;
            var caret = Caret;

            for (var i = 1; i <= masked.Length; i++)
            {
                text = masked.Substring(0, i);
                x = Offset + Gui.Renderer.GetTextSize(text, font).x;
                if (x > p.x)
                {
                    SelectEnd = i - 1;
                    break;
                }
            }

            if (x < p.x)
            {
                SelectEnd = masked.Length;
            }

            var start = Math.Min(SelectStart, SelectEnd);
            var end = Math.Max(SelectStart, SelectEnd);

            if (SelectEnd < SelectStart)
            {
                Caret = start;
            }
            else
            {
                Caret = end;
            }
        }

        private void TextBox_MouseDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0)
            {
                return;
            }

            if (!HasFocus)
            {
                SavedText = Text;
                HasFocus = true;
            }

            var style = Desktop.GetStyle(Style).Styles[State];

            var masked = Text;
            if (IsPassword)
            {
                masked = new string(PasswordChar, masked.Length);
            }

            if (string.IsNullOrEmpty(masked))
            {
                return;
            }

            var font = Gui.Renderer.GetFont(style.Font);
            if (font < 0)
            {
                return;
            }

            var p = Gui.MousePosition - Location;
            var s1 = Gui.Renderer.GetTextSize(masked, font);
            var carex = p.x + Offset + s1.x;
            var x = 0;

            var text = string.Empty;

            for (var i = 1; i <= masked.Length; i++)
            {
                text = masked.Substring(0, i);
                x = Offset + Gui.Renderer.GetTextSize(text, font).x;
                if (x > p.x)
                {
                    Caret = i - 1;
                    break;
                }
            }

            if (x < p.x)
            {
                Caret = masked.Length;
            }

            if (Gui.CtrlPressed)
            {
                var left = FindIndexLeft(Caret, masked);
                var right = FindIndexRight(Caret, masked);

                if (char.IsWhiteSpace(masked, left) || char.IsPunctuation(masked, left))
                {
                    left++;
                }

                if (char.IsWhiteSpace(masked, right - 1) || char.IsPunctuation(masked, right - 1))
                {
                    right--;
                }

                SelectStart = left;
                SelectEnd = right;

                Caret = SelectEnd;
            }
            else if (Gui.ShiftPressed)
            {
                SelectEnd = Caret;
            }
            else
            {
                SelectStart = Caret;
                SelectEnd = Caret;
            }
        }

        private int FindIndexLeft(int start, string text)
        {
            if (start <= 0)
            {
                return start;
            }

            while (true)
            {
                start--;

                if (char.IsWhiteSpace(text, start) || char.IsPunctuation(text, start) || start <= 0)
                {
                    return start;
                }
            }
        }

        private int FindIndexRight(int start, string text)
        {
            if (start >= text.Length - 1)
            {
                return start;
            }

            while (true)
            {
                start++;

                if (char.IsWhiteSpace(text, start) || char.IsPunctuation(text, start) || start >= text.Length - 1)
                {
                    return start + 1;
                }
            }
        }

        private void HandleRightArrow()
        {
            if (Gui.CtrlPressed)
            {
                var masked = Text;
                if (IsPassword)
                {
                    masked = new string(PasswordChar, masked.Length);
                }

                if (Gui.ShiftPressed)
                {
                    if (SelectEnd == Caret)
                    {
                        var index = FindIndexRight(SelectEnd, masked);

                        Caret = SelectEnd = index;
                    }
                    else if (SelectStart == Caret)
                    {
                        var index = FindIndexRight(SelectStart, masked);
                        if (index > SelectEnd)
                        {
                            SelectStart = SelectEnd;
                            SelectEnd = index;
                            Caret = index;
                        }
                        else
                        {
                            Caret = SelectStart = index;
                        }
                    }
                }
                else
                {
                    SelectStart = SelectEnd = Caret = FindIndexRight(Caret, masked);
                }
            }
            else
            {
                if (Caret < Text.Length)
                {
                    if (Gui.ShiftPressed)
                    {
                        if (SelectEnd == Caret)
                        {
                            SelectEnd++;
                        }
                        else if (SelectStart == Caret)
                        {
                            SelectStart++;
                        }

                        Caret++;
                    }
                    else
                    {
                        if (IsSelection)
                        {
                            SelectStart = SelectEnd = Caret;
                        }
                        else
                        {
                            Caret++;
                            SelectStart = SelectEnd = Caret;
                        }
                    }
                }
                else if (!Gui.ShiftPressed)
                {
                    SelectStart = SelectEnd = Caret;
                }
            }
        }

        private void HandleLeftArrow()
        {
            if (Gui.CtrlPressed)
            {
                var masked = Text;
                if (IsPassword)
                {
                    masked = new string(PasswordChar, masked.Length);
                }

                if (Gui.ShiftPressed)
                {
                    if (SelectStart == Caret)
                    {
                        Caret = SelectStart = FindIndexLeft(SelectStart, masked);
                    }
                    else if (SelectEnd == Caret)
                    {
                        var index = FindIndexLeft(SelectEnd, masked);

                        if (index < SelectStart)
                        {
                            SelectEnd = SelectStart;
                            SelectStart = index;
                            Caret = index;
                        }
                        else
                        {
                            Caret = SelectEnd = index;
                        }
                    }
                }
                else
                {
                    SelectStart = SelectEnd = Caret = FindIndexLeft(Caret, masked);
                }
            }
            else
            {
                if (Caret > 0)
                {
                    if (Gui.ShiftPressed)
                    {
                        if (SelectStart == Caret)
                        {
                            SelectStart--;
                        }
                        else if (SelectEnd == Caret)
                        {
                            SelectEnd--;
                        }

                        Caret--;
                    }
                    else
                    {
                        if (IsSelection)
                        {
                            SelectStart = SelectEnd = Caret;
                        }
                        else
                        {
                            Caret--;
                            SelectStart = SelectEnd = Caret;
                        }
                    }
                }
                else if (!Gui.ShiftPressed)
                {
                    SelectStart = SelectEnd = Caret;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            // UnityEngine.Debug.Log(args.Key);

            var root = Desktop;
            if (root == null)
            {
                return;
            }

            BlinkTime = 0; DoBlink = 1;

            if (ReadOnly)
            {
                return;
            }

            if (args.Key == Keys.HOME && !ReadOnly) // home
            {
                if (Gui.ShiftPressed)
                {
                    if (SelectStart == Caret)
                    {
                        Caret = SelectStart = 0;
                    }
                    else if (SelectEnd == Caret)
                    {
                        if (0 < SelectStart)
                        {
                            SelectEnd = SelectStart;
                            SelectStart = 0;
                            Caret = 0;
                        }
                        else
                        {
                            Caret = SelectEnd = 0;
                        }
                    }
                }
                else
                {
                    Caret = 0;
                    SelectStart = SelectEnd = Caret;
                }
            }
            else if (args.Key == Keys.END && !ReadOnly) // end
            {
                if (Gui.ShiftPressed)
                {
                    if (SelectEnd == Caret)
                    {
                        Caret = SelectEnd = Text.Length;
                    }
                    else if (SelectStart == Caret)
                    {
                        if (Text.Length > SelectEnd)
                        {
                            SelectStart = SelectEnd;
                            SelectEnd = Text.Length;
                            Caret = Text.Length;
                        }
                        else
                        {
                            Caret = SelectStart = Text.Length;
                        }
                    }
                }
                else
                {
                    Caret = Text.Length;
                    SelectStart = SelectEnd = Caret;
                }
            }
            else if (args.Key == Keys.RIGHTARROW && !ReadOnly) // right arrow
            {
                HandleRightArrow();
            }
            else if (args.Key == Keys.LEFTARROW && !ReadOnly) // left arrow
            {
                HandleLeftArrow();
            }
            else if (args.Key == Keys.BACKSPACE && !ReadOnly) // backspace
            {
                if (IsSelection)
                {
                    var start = Math.Min(SelectStart, SelectEnd);
                    var end = Math.Max(SelectStart, SelectEnd);

                    Text = Text.Remove(start, end - start);
                    Caret = start;
                    Offset = 0;
                }
                else
                {
                    if (Caret > 0)
                    {
                        Text = Text.Remove(Caret - 1, 1);
                        if (Caret > 0)
                        {
                            Caret--;
                        }
                    }
                }

                SelectStart = SelectEnd = Caret;
            }
            else if (args.Key == Keys.DELETE && !ReadOnly) // delete
            {
                if (IsSelection)
                {
                    var start = Math.Min(SelectStart, SelectEnd);
                    var end = Math.Max(SelectStart, SelectEnd);

                    Text = Text.Remove(start, end - start);
                    Caret = start;
                    Offset = 0;
                }
                else
                {
                    if (Text.Length > Caret)
                    {
                        Text = Text.Remove(Caret, 1);
                    }
                }

                SelectStart = SelectEnd = Caret;
            }
            else if (args.Key == Keys.RETURN || args.Key == Keys.NUMPADENTER) // return/enter
            {
                LostFocus -= TextBox_LostFocus;

                root.FocusedControl = null;
                Caret = 0;

                SelectStart = SelectEnd = Caret;

                LostFocus += TextBox_LostFocus;

                if (TextCommit != null)
                {
                    TextCommit(this, null);
                }
            }
            else if (args.Key == Keys.ESCAPE)
            {
                LostFocus -= TextBox_LostFocus;

                Text = SavedText;
                root.FocusedControl = null;
                Caret = 0;
                HasFocus = false;
                SelectStart = SelectEnd = Caret;

                LostFocus += TextBox_LostFocus;

                if (TextCancel != null)
                {
                    TextCancel(this, null);
                }
            }
            else
            {
                if (Gui.CtrlPressed && !Gui.AltPressed)
                {
                    if (args.Key == Keys.A) // select all
                    {
                        SelectStart = 0;
                        SelectEnd = Text.Length;
                        Caret = Text.Length;
                    }
                    else if (args.Key == Keys.C) // copy
                    {
                        if (IsSelection)
                        {
                            Gui.SetClipboard(Selection);
                        }
                    }
                    else if (args.Key == Keys.X) // copy
                    {
                        if (IsSelection)
                        {
                            Gui.SetClipboard(Selection);

                            var start = Math.Min(SelectStart, SelectEnd);
                            var end = Math.Max(SelectStart, SelectEnd);

                            Text = Text.Remove(start, end - start);
                            Caret = start;
                            Offset = 0;
                        }
                    }
                    else if (args.Key == Keys.V && !ReadOnly) // paste
                    {
                        var paste = Gui.GetClipboard();
                        if (!string.IsNullOrEmpty(paste))
                        {
                            if (IsSelection)
                            {
                                var start = Math.Min(SelectStart, SelectEnd);
                                var end = Math.Max(SelectStart, SelectEnd);

                                Text = Text.Remove(start, end - start);
                                Caret = start;
                            }

                            Text = Text.Insert(Caret, paste.ToString());
                            if (Caret < Text.Length)
                            {
                                Caret += paste.Length;
                            }

                            SelectStart = SelectEnd = Caret;
                        }
                    }
                }
                else
                {
                    if (args.Key != Keys.TAB)
                    {
                        if (args.Char.HasValue)
                        {
                            var valid = true;
                            var c = args.Char.Value;

                            if (Mode == TextBoxMode.Numeric)
                            {
                                valid = char.IsNumber(c) || char.IsDigit(c) || (c.ToString() == ".") || (c.ToString() == ",");
                            }

                            if (valid)
                            {
                                if (IsSelection)
                                {
                                    var start = Math.Min(SelectStart, SelectEnd);
                                    var end = Math.Max(SelectStart, SelectEnd);

                                    Text = Text.Remove(start, end - start);
                                    Caret = start;
                                }

                                Text = Text.Insert(Caret, c.ToString());
                                if (Caret < Text.Length)
                                {
                                    Caret++;
                                }

                                SelectStart = SelectEnd = Caret;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            BlinkTime += Gui.TimeElapsed;

            if (BlinkTime > BlinkInterval)
            {
                DoBlink = 1 - DoBlink;
                BlinkTime = 0;
            }
        }

        protected override void DrawText(Style style, float opacity)
        {
            if (_text == null)
            {
                _text = string.Empty;
            }

            var masked = _text;
            if (IsPassword)
            {
                masked = new string(PasswordChar, masked.Length);
            }

            var font = Gui.Renderer.GetFont(style.Font);
            if (font < 0)
            {
                return;
            }

            var p = AlignText(masked, Alignment.MiddleLeft, style.TextPadding, font);

            var clip = new Rectangle(Location, Size);
            clip.Left += style.TextPadding.Left;
            clip.Right -= style.TextPadding.Right - 1;
            clip = Clip(clip);

            if (clip.Width < 1 || clip.Height < 1)
            {
                return;
            }

            SetScissor(clip.Left, clip.Top, clip.Width, clip.Height);

            if (Caret > masked.Length)
            {
                Caret = masked.Length;
            }

            if (Desktop.FocusedControl == this)
            {
                var rect = new Rectangle(Location, Size);

                var s1 = Gui.Renderer.GetTextSize(masked, font);
                var s2 = Gui.Renderer.GetTextSize(masked.Substring(0, Caret), font);

                if (string.IsNullOrEmpty(masked))
                {
                    s2.y = Gui.Renderer.GetTextSize(" ", font).y;
                    p = AlignText(" ", Alignment.MiddleLeft, style.TextPadding, font);
                }
                else if (s2.y == 0)
                {
                    s2.y = Gui.Renderer.GetTextSize(" ", font).y;
                }

                var carex = p.x + Offset + s2.x;

                var lim1 = rect.Left + style.TextPadding.Left;
                var lim2 = rect.Right - style.TextPadding.Right;

                if (carex < lim1)
                {
                    Offset += lim1 - carex;
                }

                if (carex > lim2)
                {
                    Offset += lim2 - carex;
                }

                if (Offset < 0)
                {
                    if (p.x + Offset + s1.x < lim2)
                    {
                        Offset += lim2 - (p.x + Offset + s1.x);
                    }
                }

                p.x += Offset;

                Gui.Renderer.DrawText(masked, p.x, p.y, font, ColorInt.FromArgb(opacity, UseTextColor ? TextColor : style.TextColor));

                if (!ReadOnly && DoBlink > 0)
                {
                    Gui.Renderer.DrawBox(p.x + s2.x, p.y, 1, s2.y, ColorInt.FromArgb(opacity, BlinkColor));
                }

                if (IsSelection)
                {
                    var start = Math.Min(SelectStart, SelectEnd);
                    var end = Math.Max(SelectStart, SelectEnd);
                    var color = ColorInt.FromArgb(0.5f, SelectionColor);
                    var text = masked.Substring(0, start);
                    var text2 = masked.Substring(start, end - start);

                    var size1 = Gui.Renderer.GetTextSize(text, font);
                    var size2 = Gui.Renderer.GetTextSize(text2, font);

                    Gui.Renderer.DrawBox(p.x + size1.x, p.y, size2.x, size2.y, ColorInt.FromArgb(opacity, color));
                }
            }
            else
            {
                HasFocus = false;
                Offset = 0;
                Gui.Renderer.DrawText(masked, p.x, p.y, font, ColorInt.FromArgb(opacity, style.TextColor));
            }

            ResetScissor();
        }
    }
}
