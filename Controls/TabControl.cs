using System;

namespace Squid
{
    /// <summary>
    /// A collection of TabPages
    /// </summary>
    public class TabPageCollection : ActiveList<TabPage> { }

    /// <summary>
    /// A TabControl
    /// </summary>
    [Toolbox]
    public class TabControl : Control
    {
        private TabPage _selectedTab;

        /// <summary>
        /// Gets the button frame.
        /// </summary>
        /// <value>The button frame.</value>
        public Frame ButtonFrame { get; private set; }

        /// <summary>
        /// Gets the tab pages.
        /// </summary>
        /// <value>The tab pages.</value>
        public TabPageCollection TabPages { get; private set; }

        /// <summary>
        /// Gets or sets the selected tab.
        /// </summary>
        /// <value>The selected tab.</value>
        public TabPage SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab == value)
                {
                    return;
                }

                if (_selectedTab != null)
                {
                    _selectedTab.Button.Checked = false;
                    Elements.Remove(_selectedTab);
                }

                if (value != null && !TabPages.Contains(value))
                {
                    _selectedTab = null;
                    return;
                }

                _selectedTab = value;

                if (_selectedTab != null)
                {
                    _selectedTab.Button.Checked = true;
                    Elements.Add(_selectedTab);
                }

                //if (OnSelectedTabChanged != null)
                //    OnSelectedTabChanged(_selectedTab);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabControl"/> class.
        /// </summary>
        public TabControl()
        {
            TabPages = new TabPageCollection();
            TabPages.ItemAdded += TabPages_ItemAdded;
            TabPages.ItemRemoved += TabPages_ItemRemoved;
            TabPages.BeforeItemsCleared += TabPages_BeforeItemsCleared;

            Size = new Point(100, 100);

            ButtonFrame = new Frame
            {
                Size = new Point(100, 35),
                Dock = DockStyle.Top,
                Scissor = true
            };
            Elements.Add(ButtonFrame);
        }

        private void TabPages_BeforeItemsCleared(object sender, EventArgs e)
        {
            foreach (var page in TabPages)
            {
                page.Button.MouseClick -= Button_MouseClick;
                page.Button.Parent = ButtonFrame;
            }
        }

        private void TabPages_ItemRemoved(object sender, ListEventArgs<TabPage> e)
        {
            e.Item.Button.MouseClick -= Button_MouseClick;
            e.Item.Button.Parent = null;
        }

        private void TabPages_ItemAdded(object sender, ListEventArgs<TabPage> e)
        {
            ButtonFrame.Controls.Clear();

            foreach (var page in TabPages)
            {
                page.Button.MouseClick -= Button_MouseClick;
                page.Button.MouseClick += Button_MouseClick;
                page.Button.Parent = ButtonFrame;
            }

            SelectedTab = e.Item;
        }

        private void Button_MouseClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0)
            {
                return;
            }

            SelectedTab = sender.Owner as TabPage;
        }
    }

    /// <summary>
    /// A TabPage
    /// </summary>
    public class TabPage : Frame
    {
        /// <summary>
        /// Gets the button.
        /// </summary>
        /// <value>The button.</value>
        public TabButton Button { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabPage"/> class.
        /// </summary>
        public TabPage()
        {
            Scissor = true;
            Dock = DockStyle.Fill;

            Button = new TabButton
            {
                Size = new Point(60, 35),
                Owner = this,
                Dock = DockStyle.Left
            };
        }
    }

    /// <summary>
    /// A TabButton
    /// </summary>
    public class TabButton : Button, IControlContainer
    {
        public ControlCollection Controls { get; set; }
    }

}
