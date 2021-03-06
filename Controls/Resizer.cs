﻿namespace Squid
{
    /// <summary>
    /// Delegate ResizeHandler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="delta">The delta.</param>
    /// <param name="moved">The moved.</param>
    public delegate void ResizeHandler(Control sender, Point delta, Point moved);

    /// <summary>
    /// This control provides handles to resize its parent.
    /// </summary>
    [Hidden]
    public sealed class Resizer : Control
    {
        public Control Left { get; private set; }
        public Control Right { get; private set; }

        public Control Top { get; private set; }
        public Control TopLeft { get; private set; }
        public Control TopRight { get; private set; }

        public Control Bottom { get; private set; }
        public Control BottomLeft { get; private set; }
        public Control BottomRight { get; private set; }

        private Margin _grip = new Margin(8);
        private Point ClickedPos;
        private Point OldSize;

        public event MouseEvent GripDown;
        public event MouseEvent GripUp;
        public event ResizeHandler Resized;

        public Margin GripSize
        {
            get => _grip;
            set
            {
                if (_grip == value)
                {
                    return;
                }

                _grip = value;
                Adjust();
            }
        }

        public Resizer()
        {
            NoEvents = true;
            Dock = DockStyle.Fill;

            Left = new Control
            {
                Size = new Point(2, 2),
                Dock = DockStyle.Left
            };
            Left.MouseDown += Grip_OnDown;
            Left.MousePress += Grip_OnPress;
            Left.MouseUp += Grip_OnUp;
            Left.Tag = AnchorStyles.Left;
            Left.Cursor = CursorNames.SizeWE;
            Elements.Add(Left);

            Top = new Control
            {
                Size = new Point(2, 2),
                Dock = DockStyle.Top
            };
            Top.MouseDown += Grip_OnDown;
            Top.MousePress += Grip_OnPress;
            Top.MouseUp += Grip_OnUp;
            Top.Tag = AnchorStyles.Top;
            Top.Cursor = CursorNames.SizeNS;
            Elements.Add(Top);

            Right = new Control
            {
                Size = new Point(2, 2),
                Dock = DockStyle.Right
            };
            Right.MouseDown += Grip_OnDown;
            Right.MousePress += Grip_OnPress;
            Right.MouseUp += Grip_OnUp;
            Right.Tag = AnchorStyles.Right;
            Right.Cursor = CursorNames.SizeWE;
            Elements.Add(Right);

            Bottom = new Control
            {
                Size = new Point(2, 2),
                Dock = DockStyle.Bottom
            };
            Bottom.MouseDown += Grip_OnDown;
            Bottom.MousePress += Grip_OnPress;
            Bottom.MouseUp += Grip_OnUp;
            Bottom.Tag = AnchorStyles.Bottom;
            Bottom.Cursor = CursorNames.SizeNS;
            Elements.Add(Bottom);

            TopLeft = new Control
            {
                Size = new Point(4, 4),
                Position = new Point(0, 0)
            };
            TopLeft.MouseDown += Grip_OnDown;
            TopLeft.MousePress += Grip_OnPress;
            TopLeft.MouseUp += Grip_OnUp;
            TopLeft.Tag = AnchorStyles.Top | AnchorStyles.Left;
            TopLeft.Cursor = CursorNames.SizeNWSE;
            Elements.Add(TopLeft);

            TopRight = new Control
            {
                Size = new Point(4, 4),
                Position = new Point(Size.x - 4, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            TopRight.MouseDown += Grip_OnDown;
            TopRight.MousePress += Grip_OnPress;
            TopRight.MouseUp += Grip_OnUp;
            TopRight.Tag = AnchorStyles.Top | AnchorStyles.Right;
            TopRight.Cursor = CursorNames.SizeNESW;
            Elements.Add(TopRight);

            BottomLeft = new Control
            {
                Size = new Point(4, 4),
                Position = new Point(0, Size.y - 4),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            BottomLeft.MouseDown += Grip_OnDown;
            BottomLeft.MousePress += Grip_OnPress;
            BottomLeft.MouseUp += Grip_OnUp;
            BottomLeft.Tag = AnchorStyles.Bottom | AnchorStyles.Left;
            BottomLeft.Cursor = CursorNames.SizeNESW;
            Elements.Add(BottomLeft);

            BottomRight = new Control
            {
                Size = new Point(8, 8),
                Position = new Point(Size.x - 8, Size.y - 8),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            BottomRight.MouseDown += Grip_OnDown;
            BottomRight.MousePress += Grip_OnPress;
            BottomRight.MouseUp += Grip_OnUp;
            BottomRight.Tag = AnchorStyles.Bottom | AnchorStyles.Right;
            BottomRight.Cursor = CursorNames.SizeNWSE;
            Elements.Add(BottomRight);

            Adjust();
        }

        private void Adjust()
        {
            Left.Size = new Point(_grip.Left, _grip.Left);
            Top.Size = new Point(_grip.Top, _grip.Top);
            Right.Size = new Point(_grip.Right, _grip.Right);
            Bottom.Size = new Point(_grip.Bottom, _grip.Bottom);

            TopLeft.Size = new Point(_grip.Left, _grip.Top);
            TopLeft.Position = new Point(0, 0);

            TopRight.Size = new Point(_grip.Right, _grip.Top);
            TopRight.Position = new Point(Size.x - _grip.Right, 0);

            BottomLeft.Size = new Point(_grip.Left, _grip.Bottom);
            BottomLeft.Position = new Point(0, Size.y - _grip.Bottom);

            BottomRight.Size = new Point(_grip.Right, _grip.Bottom);
            BottomRight.Position = new Point(Size.x - _grip.Right, Size.y - _grip.Bottom);
        }

        private void Grip_OnDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0)
            {
                return;
            }

            ClickedPos = Gui.MousePosition;
            OldSize = Parent.Size;

            if (GripDown != null)
            {
                GripDown(sender, args);
            }
        }

        private void Grip_OnUp(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0)
            {
                return;
            }

            if (GripUp != null)
            {
                GripUp(sender, args);
            }
        }

        private void Grip_OnPress(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0)
            {
                return;
            }

            var p = Gui.MousePosition - ClickedPos;

            var position = Parent.Position;
            var size = Parent.Size;

            var anchor = (AnchorStyles)sender.Tag;

            if ((anchor & AnchorStyles.Left) == AnchorStyles.Left)
            {
                p.x = ClickedPos.x - Gui.MousePosition.x;
            }

            if ((anchor & AnchorStyles.Top) == AnchorStyles.Top)
            {
                p.y = ClickedPos.y - Gui.MousePosition.y;
            }

            Parent.ResizeTo(OldSize + p, anchor);

            if (Resized != null)
            {
                Resized(this, Parent.Size - size, Parent.Position - position);
            }
        }
    }
}
