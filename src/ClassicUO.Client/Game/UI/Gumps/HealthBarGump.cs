#region license

// Copyright (c) 2024, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.IO;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;
using ClassicUO.Game.Scenes;
using ClassicUO.Utility.Logging;
using System.Diagnostics;

namespace ClassicUO.Game.UI.Gumps
{
    internal abstract class BaseHealthBarGump : AnchorableGump
    {
        private bool _targetBroke;

        protected BaseHealthBarGump(World world, Entity entity) : this(world, 0, 0)
        {
            //if (entity == null || entity.IsDestroyed)
            //{
            //    Dispose();

            //    return;
            //}

            //GameActions.RequestMobileStatus(world, entity.Serial, true);
            LocalSerial = 0;
            CanCloseWithRightClick = true;
            _name = "test name"; //entity.Name;
            _isDead = false; //entity is Mobile mm && mm.IsDead;

            BuildGump();
        }

        protected BaseHealthBarGump(World world, uint serial, String name) : this(world, 0, 0)
        {
            //if (entity == null || entity.IsDestroyed)
            //{
            //    Dispose();

            //    return;
            //}

            //GameActions.RequestMobileStatus(world, entity.Serial, true);
            LocalSerial = serial;
            CanCloseWithRightClick = true;
            _name = name; //entity.Name;
            _isDead = false; //entity is Mobile mm && mm.IsDead;

            BuildGump();
        }

        protected BaseHealthBarGump(World world, uint serial) : this(world, null)
        {
        }

        protected BaseHealthBarGump(World world, uint local, uint server) : base(world, local, server)
        {
            CanMove = true;
            AnchorType = ANCHOR_TYPE.HEALTHBAR;
        }

        public override int GroupMatrixWidth
        {
            get => Width;
            protected set { }
        }

        public override int GroupMatrixHeight
        {
            get => Height;
            protected set { }
        }

        public override GumpType GumpType => GumpType.HealthBar;
        internal bool IsInactive => (_isDead || _outOfRange) && !_canChangeName;
        protected bool _canChangeName;
        protected bool _isDead;
        protected string _name;
        protected bool _outOfRange;
        protected StbTextBox _textBox;

        protected abstract void BuildGump();


        public override void Dispose()
        {
            /*if (TargetManager.LastAttack != LocalSerial)
            {
                GameActions.SendCloseStatus(LocalSerial);
            }*/
            
            _textBox?.Dispose();
            _textBox = null;
            base.Dispose();
        }

        public override void Save(XmlTextWriter writer)
        {
            base.Save(writer);

            if (ProfileManager.CurrentProfile.SaveHealthbars)
            {
                writer.WriteAttributeString("name", _name);
            }
        }

        public override void Restore(XmlElement xml)
        {
            base.Restore(xml);

            if (LocalSerial == World.Player)
            {
                _name = World.Player.Name;
                BuildGump();
            }
            else if (ProfileManager.CurrentProfile.SaveHealthbars)
            {
                _name = xml.GetAttribute("name");
                _outOfRange = true;
                BuildGump();
            }
            else
            {
                Dispose();
            }
        }

        protected void TextBoxOnMouseUp(object sender, MouseEventArgs e)
        {
            return;
            if (e.Button != MouseButtonType.Left)
            {
                return;
            }

            if (World.Get(LocalSerial) == null)
            {
                return;
            }

            Point p = Mouse.LDragOffset;

            if (Math.Max(Math.Abs(p.X), Math.Abs(p.Y)) >= 1)
            {
                return;
            }

            if (World.TargetManager.IsTargeting)
            {
                World.TargetManager.Target(LocalSerial);
                Mouse.LastLeftButtonClickTime = 0;
            }
            else if (_canChangeName && !_targetBroke)
            {
                _textBox.IsEditable = true;
                _textBox.SetKeyboardFocus();
            }

            _targetBroke = false;
        }

        protected static int CalculatePercents(int max, int current, int maxValue)
        {
            if (max > 0)
            {
                max = current * 100 / max;

                if (max > 100)
                {
                    max = 100;
                }

                if (max > 1)
                {
                    max = maxValue * max / 100;
                }
            }

            return max;
        }

        protected override void OnDragEnd(int x, int y)
        {
            base.OnDragEnd(x, y);
        }

        protected override void OnMouseDown(int x, int y, MouseButtonType button)
        {
            if (button != MouseButtonType.Left)
            {
                return;
            }

            //if (World.TargetManager.IsTargeting)
            //{
            //    _targetBroke = true;
            //    World.TargetManager.Target(LocalSerial);
            //    Mouse.LastLeftButtonClickTime = 0;
            //}
            //else if (_canChangeName)
            //{
            //    if (_textBox != null)
            //    {
            //        _textBox.IsEditable = false;
            //    }

            //    UIManager.KeyboardFocusControl = null;
            //    UIManager.SystemChat?.SetFocus();
            //}

            base.OnMouseDown(x, y, button);
        }

        protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
        {
            return false;
            if (button != MouseButtonType.Left)
            {
                return false;
            }

            if (_canChangeName)
            {
                if (_textBox != null)
                {
                    _textBox.IsEditable = false;
                }

                UIManager.KeyboardFocusControl = null;
                UIManager.SystemChat?.SetFocus();
            }

            Entity entity = World.Get(LocalSerial);

            if (entity != null)
            {
                if (entity != World.Player)
                {
                    if (World.Player.InWarMode)
                    {
                        GameActions.Attack(World, entity);
                    }
                    else if (!GameActions.OpenCorpse(World, entity))
                    {
                        GameActions.DoubleClick(World, entity);
                    }
                }
                else
                {
                    UIManager.Add(StatusGumpBase.AddStatusGump(World, ScreenCoordinateX, ScreenCoordinateY));
                    Dispose();
                }
            }

            return true;
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            Entity entity = null;// World.Get(LocalSerial);

            if (entity == null || SerialHelper.IsItem(entity.Serial))
            {
                return;
            }

            if ((key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER) && _textBox != null && _textBox.IsEditable)
            {
                GameActions.Rename(entity, _textBox.Text);
                UIManager.KeyboardFocusControl = null;
                UIManager.SystemChat?.SetFocus();
                _textBox.IsEditable = false;
            }
        }

        protected override void OnMouseOver(int x, int y)
        {
            //Entity entity = World.Get(LocalSerial);

            //if (entity != null)
            //{
            //    SelectedObject.HealthbarObject = entity;
            //    SelectedObject.Object = entity;
            //}

            base.OnMouseOver(x, y);
        }


        protected bool CheckIfAnchoredElseDispose()
        {
            if (UIManager.AnchorManager[this] == null && LocalSerial != World.Player)
            {
                Dispose();

                return true;
            }

            return false;
        }
    }

    internal class HealthBarGumpCustom : BaseHealthBarGump
    {
        internal const int HPB_WIDTH = 120;
        internal const int HPB_HEIGHT_MULTILINE = 60;
        internal const int HPB_HEIGHT_SINGLELINE = 36;
        private const int HPB_BORDERSIZE = 1;
        private const int HPB_OUTLINESIZE = 1;


        internal const int HPB_BAR_WIDTH = 100;
        private const int HPB_BAR_HEIGHT = 8;
        private const int HPB_BAR_SPACELEFT = (HPB_WIDTH - HPB_BAR_WIDTH) / 2;


        private static Color HPB_COLOR_DRAW_RED = Color.Red;
        private static Color HPB_COLOR_DRAW_BLUE = Color.DodgerBlue;
        private static Color HPB_COLOR_DRAW_BLACK = Color.Black;

        private static readonly Texture2D HPB_COLOR_BLUE = SolidColorTextureCache.GetTexture(Color.DodgerBlue);
        private static readonly Texture2D HPB_COLOR_GRAY = SolidColorTextureCache.GetTexture(Color.Gray);
        private static readonly Texture2D HPB_COLOR_RED = SolidColorTextureCache.GetTexture(Color.Red);
        private static readonly Texture2D HPB_COLOR_YELLOW = SolidColorTextureCache.GetTexture(Color.Orange);
        private static readonly Texture2D HPB_COLOR_POISON = SolidColorTextureCache.GetTexture(Color.LimeGreen);
        private static readonly Texture2D HPB_COLOR_BLACK = SolidColorTextureCache.GetTexture(Color.Black);

        private readonly LineCHB[] _bars = new LineCHB[3];
        private readonly LineCHB[] _border = new LineCHB[4];

        private LineCHB _hpLineRed, _manaLineRed, _stamLineRed, _outline;


        private bool _oldWarMode, _normalHits, _poisoned, _yellowHits;

        public HealthBarGumpCustom(World world, Entity entity) : base(world, entity)
        {
        }
        public HealthBarGumpCustom(World world, uint serial, String name) : base(world, serial, name)
        {
        }

        public HealthBarGumpCustom(World world, uint serial) : base(world, serial)
        {
        }

        public HealthBarGumpCustom(World world) : base(world, 0, 0)
        {
        }

        protected AlphaBlendControl _background;


        protected override void UpdateContents()
        {
            Clear();
            Children.Clear();

            _background = null;
            _hpLineRed = _manaLineRed = _stamLineRed = null;

            if (_textBox != null)
            {
                _textBox.MouseUp -= TextBoxOnMouseUp;
            }

            _textBox = null;

            BuildGump();
        }


        public override void Update()
        {
            base.Update();

            if (IsDisposed)
            {
                return;
            }

        }

        protected override void BuildGump()
        {
            WantUpdateSize = false;


            _oldWarMode = false;
            Height = HPB_HEIGHT_MULTILINE;
            Width = HPB_WIDTH;

            Add(_background = new AlphaBlendControl(0.7f) { Width = Width, Height = Height, AcceptMouseInput = true, CanMove = true });

            Add
            (
                _textBox = new StbTextBox
                (
                    1,
                    32,
                    isunicode: true,
                    style: FontStyle.Cropped | FontStyle.BlackBorder,
                    hue: 0x005A,
                    maxWidth: Width,
                    align: TEXT_ALIGN_TYPE.TS_CENTER
                )
                {
                    X = 0,
                    Y = 3,
                    Width = HPB_BAR_WIDTH,
                    IsEditable = false,
                    CanMove = true
                }
            );

            Add
            (
                _outline = new LineCHB
                (
                    HPB_BAR_SPACELEFT - HPB_OUTLINESIZE,
                    27 - HPB_OUTLINESIZE,
                    HPB_BAR_WIDTH + HPB_OUTLINESIZE * 2,
                    HPB_BAR_HEIGHT * 3 + 2 + HPB_OUTLINESIZE * 2,
                    HPB_COLOR_DRAW_BLACK.PackedValue
                )
            );

            Add
            (
                _hpLineRed = new LineCHB
                (
                    HPB_BAR_SPACELEFT,
                    27,
                    HPB_BAR_WIDTH,
                    HPB_BAR_HEIGHT,
                    HPB_COLOR_DRAW_RED.PackedValue
                )
            );

            Add
            (
                new LineCHB
                (
                    HPB_BAR_SPACELEFT,
                    36,
                    HPB_BAR_WIDTH,
                    HPB_BAR_HEIGHT,
                    HPB_COLOR_DRAW_RED.PackedValue
                )
            );

            Add
            (
                new LineCHB
                (
                    HPB_BAR_SPACELEFT,
                    45,
                    HPB_BAR_WIDTH,
                    HPB_BAR_HEIGHT,
                    HPB_COLOR_DRAW_RED.PackedValue
                )
            );

            Add
            (
                _bars[0] = new LineCHB
                (
                    HPB_BAR_SPACELEFT,
                    27,
                    HPB_BAR_WIDTH,
                    HPB_BAR_HEIGHT,
                    HPB_COLOR_DRAW_BLUE.PackedValue
                ) { LineWidth = 0 }
            );

            Add
            (
                _bars[1] = new LineCHB
                (
                    HPB_BAR_SPACELEFT,
                    36,
                    HPB_BAR_WIDTH,
                    HPB_BAR_HEIGHT,
                    HPB_COLOR_DRAW_BLUE.PackedValue
                ) { LineWidth = 0 }
            );

            Add
            (
                _bars[2] = new LineCHB
                (
                    HPB_BAR_SPACELEFT,
                    45,
                    HPB_BAR_WIDTH,
                    HPB_BAR_HEIGHT,
                    HPB_COLOR_DRAW_BLUE.PackedValue
                ) { LineWidth = 0 }
            );

            Add
            (
                _border[0] = new LineCHB
                (
                    0,
                    0,
                    HPB_WIDTH,
                    HPB_BORDERSIZE,
                    HPB_COLOR_DRAW_BLACK.PackedValue
                )
            );

            Add
            (
                _border[1] = new LineCHB
                (
                    0,
                    HPB_HEIGHT_MULTILINE - HPB_BORDERSIZE,
                    HPB_WIDTH,
                    HPB_BORDERSIZE,
                    HPB_COLOR_DRAW_BLACK.PackedValue
                )
            );

            Add
            (
                _border[2] = new LineCHB
                (
                    0,
                    0,
                    HPB_BORDERSIZE,
                    HPB_HEIGHT_MULTILINE,
                    HPB_COLOR_DRAW_BLACK.PackedValue
                )
            );

            Add
            (
                _border[3] = new LineCHB
                (
                    HPB_WIDTH - HPB_BORDERSIZE,
                    0,
                    HPB_BORDERSIZE,
                    HPB_HEIGHT_MULTILINE,
                    HPB_COLOR_DRAW_BLACK.PackedValue
                )
            );

            _border[0].LineColor = _border[1].LineColor = _border[2].LineColor = _border[3].LineColor = _oldWarMode ? HPB_COLOR_RED : HPB_COLOR_BLACK;
            

            _textBox.MouseUp += TextBoxOnMouseUp;
            _textBox.SetText(_name);
        }

        public override bool Contains(int x, int y)
        {
            return true;
        }

        private class LineCHB : Line
        {
            public LineCHB(int x, int y, int w, int h, uint color) : base
            (
                x,
                y,
                w,
                h,
                color
            )
            {
                LineWidth = w;

                LineColor = SolidColorTextureCache.GetTexture(new Color { PackedValue = color });

                CanMove = true;
            }

            public int LineWidth { get; set; }
            public Texture2D LineColor { get; set; }

            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, false, Alpha);

                batcher.Draw
                (
                    LineColor,
                    new Rectangle
                    (
                        x,
                        y,
                        LineWidth,
                        Height
                    ),
                    hueVector
                );

                return true;
            }
        }

        #region Health Bar Gump Custom

        // Health Bar Gump Custom v.1c by Syrupz(Alan)
        //
        // The goal of this was to simply modernize the Health Bar Gumps while still giving people
        // an option to continue using the classic Health Bar Gumps. The option to overide bar types
        // be it (straight line(custom) or graphic(classic) is directly included in this version
        // with no need to change art files in UO directory.
        //
        // Please report any problems with this to Alan#0084 on Discord and I will promptly work on fixing said issues.
        //
        // Lastly, I want to give a special thanks to Gaechti for helping me stress test this
        // and helping me work and organizing this in a timely fashion to get this released.
        // I would like to also thank KaRaShO, Roxya, Stalli, and Link for their input, tips, 
        // and advice to approach certain challenges that arose throughout development.
        // in different manners to get these Health Bars to function per my own vision; gratitude.
        //
        // Health Bar Gump Custom v.1c by Syrupz(Alan)

        #endregion
    }

    internal class HealthBarGump : BaseHealthBarGump
    {
        private const ushort BACKGROUND_NORMAL = 0x0803;
        private const ushort BACKGROUND_WAR = 0x0807;
        private const ushort LINE_RED = 0x0805;
        private const ushort LINE_BLUE = 0x0806;
        private const ushort LINE_POISONED = 0x0808;
        private const ushort LINE_YELLOWHITS = 0x0809;

        private const ushort LINE_RED_PARTY = 0x0028;
        private const ushort LINE_BLUE_PARTY = 0x0029;
        private GumpPic _background, _hpLineRed, _manaLineRed, _stamLineRed;

        private readonly GumpPicWithWidth[] _bars = new GumpPicWithWidth[3];

        private Button _buttonHeal1, _buttonHeal2;
        private int _oldHits, _oldStam, _oldMana;

        private bool _oldWarMode, _normalHits, _poisoned, _yellowHits;


        public HealthBarGump(World world, Entity entity) : base(world,entity)
        {
        }

        public HealthBarGump(World world, uint serial) : base(world,serial)
        {
        }

        public HealthBarGump(World world) : base(world, 0, 0)
        {
        }

        public override int GroupMatrixWidth
        {
            get => Width;
            protected set { }
        }

        public override int GroupMatrixHeight
        {
            get => Height;
            protected set { }
        }

        protected override void UpdateContents()
        {
            Clear();
            Children.Clear();

            _background = _hpLineRed = _manaLineRed = _stamLineRed = null;
            _buttonHeal1 = _buttonHeal2 = null;

            if (_textBox != null)
            {
                _textBox.MouseUp -= TextBoxOnMouseUp;
            }

            _textBox = null;

            BuildGump();
        }

        protected override void BuildGump()
        {
            WantUpdateSize = false;

            _oldWarMode = false;

            Add(_background = new GumpPic(0, 0, _oldWarMode ? BACKGROUND_WAR : BACKGROUND_NORMAL, 0) { ContainsByBounds = true });

            Width = _background.Width;
            Height = _background.Height;

            // add backgrounds
            Add(_hpLineRed = new GumpPic(34, 12, LINE_RED, 0));
            Add(new GumpPic(34, 25, LINE_RED, 0));
            Add(new GumpPic(34, 38, LINE_RED, 0));

            // add over
            Add
            (
                _bars[0] = new GumpPicWithWidth
                (
                    34,
                    12,
                    LINE_BLUE,
                    0,
                    0
                )
            );

            Add
            (
                _bars[1] = new GumpPicWithWidth
                (
                    34,
                    25,
                    LINE_BLUE,
                    0,
                    0
                )
            );

            Add
            (
                _bars[2] = new GumpPicWithWidth
                (
                    34,
                    38,
                    LINE_BLUE,
                    0,
                    0
                )
            );


            if (_textBox != null)
            {
                _textBox.MouseUp += TextBoxOnMouseUp;
                _textBox.SetText(_name);
            }
        }


        public override void Update()
        {
            base.Update();

            if (IsDisposed /* || (_textBox != null && _textBox.IsDisposed)*/)
            {
                return;
            }

        }

        public override void OnButtonClick(int buttonID)
        {
            switch ((ButtonParty) buttonID)
            {
                case ButtonParty.Heal1:
                    GameActions.CastSpell(29);
                    World.Party.PartyHealTimer = Time.Ticks + 50;
                    World.Party.PartyHealTarget = LocalSerial;

                    break;

                case ButtonParty.Heal2:
                    GameActions.CastSpell(11);
                    World.Party.PartyHealTimer = Time.Ticks + 50;
                    World.Party.PartyHealTarget = LocalSerial;

                    break;
            }

            Mouse.CancelDoubleClick = true;
            Mouse.LastLeftButtonClickTime = 0;
        }


        private enum ButtonParty
        {
            Heal1,
            Heal2
        }
    }
}