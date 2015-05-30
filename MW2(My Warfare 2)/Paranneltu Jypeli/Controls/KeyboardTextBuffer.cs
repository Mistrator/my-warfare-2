using System;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Jypeli.Controls
{
#if WINDOWS
    public sealed class KeyboardTextBuffer: System.Windows.Forms.IMessageFilter
    {
        [Flags]
        private enum KeyModifiers : int
        {
            None = 0x00,
            LeftControl = 0x01,
            RightControl = 0x02,
            Control = 0x03,
            LeftAlt = 0x04,
            RightAlt = 0x08,
            Alt = 0x0c,
            LeftShift = 0x10,
            RightShift = 0x20,
            Shift = 0x30,
        }

        private struct KeyData
        {
            public Keys Key;
            public KeyModifiers Modifier;
        }

        StringBuilder textData = new StringBuilder();
        Stack<KeyData> keyData = new Stack<KeyData>();
        KeyModifiers modifier;


        public bool Enabled { get; set; }
        public bool TranslateMessage { get; set; }
        
        public bool TabEnabled { get; set; }
        public bool BackspaceEnabled { get; set; }
        public bool MultilineEnabled { get; set; }

        public String Text { get { return textData.ToString(); } }
        public int TextLength { get { return keyData.Count; } }

        public event Action TextChanged;

        private bool inTextChanged;
        private void OnTextChanged()
        {
            if ( inTextChanged || TextChanged == null ) return;
            inTextChanged = true;
            TextChanged();
            inTextChanged = false;
        }

        public KeyboardTextBuffer()
        {
            System.Windows.Forms.Application.AddMessageFilter( this );
            TranslateMessage = true;
        }

        public void Clear()
        {
            textData.Length = 0;
            OnTextChanged();
        }

        #region IMessageFilter

        #region Enum

        protected enum Wm
        {
            Active = 6,
            Char = 0x102,
            KeyDown = 0x100,
            KeyUp = 0x101,
            SysKeyDown = 260,
            SysKeyUp = 0x105
        }

        protected enum Wa
        {
            Inactive,
            Active,
            ClickActive
        }

        protected enum Vk
        {
            Alt = 0x12,
            Control = 0x11,
            Shift = 0x10
        }

        #endregion

        #region Interop

        [System.Runtime.InteropServices.DllImport( "user32.dll", EntryPoint = "TranslateMessage" )]
        protected extern static bool _TranslateMessage( ref System.Windows.Forms.Message m );

        #endregion        

        bool System.Windows.Forms.IMessageFilter.PreFilterMessage( ref System.Windows.Forms.Message m )
        {
            if ( !Enabled )
                return false;
            //
            switch ( (Wm)m.Msg )
            {
            case Wm.SysKeyDown:
            case Wm.KeyDown:
                if ( (m.LParam.ToInt32() & (1 << 30)) == 0 )//iff repeat count == 0
                {
                    KeyData data;
                    switch ( (Vk)m.WParam )
                    {
                    case Vk.Control:
                        if ( (m.LParam.ToInt32() & (1 << 24)) == 0 )
                        {
                            data = new KeyData { Key = Keys.LeftControl, Modifier = modifier };
                            keyData.Push( data );
                            modifier |= KeyModifiers.LeftControl;
                        }
                        else
                        {
                            data = new KeyData { Key = Keys.RightControl, Modifier = modifier };
                            keyData.Push( data );
                            modifier |= KeyModifiers.RightControl;
                        }
                        break;
                    case Vk.Alt:
                        if ( (m.LParam.ToInt32() & (1 << 24)) == 0 )
                        {
                            data = new KeyData { Key = Keys.LeftAlt, Modifier = modifier };
                            keyData.Push( data );
                            modifier |= KeyModifiers.LeftAlt;
                        }
                        else
                        {
                            data = new KeyData { Key = Keys.RightAlt, Modifier = modifier };
                            keyData.Push( data );
                            modifier |= KeyModifiers.RightAlt;
                        }
                        break;
                    case Vk.Shift:
                        if ( (m.LParam.ToInt32() & (1 << 24)) == 0 )
                        {
                            data = new KeyData { Key = Keys.LeftShift, Modifier = modifier };
                            keyData.Push( data );
                            modifier |= KeyModifiers.LeftShift;
                        }
                        else
                        {
                            data = new KeyData { Key = Keys.RightShift, Modifier = modifier };
                            keyData.Push( data );
                            modifier |= KeyModifiers.RightShift;
                        }
                        break;
                    //
                    default:
                        data = new KeyData { Key = (Keys)m.WParam, Modifier = modifier };
                        keyData.Push( data );
                        break;
                    }
                }
                
                if ( TranslateMessage )
                    _TranslateMessage( ref m );

                // Allow further processing of the message
                // If true, Alt-F4 among other key combinations is disabled.
                return false;
                
            case Wm.SysKeyUp:
            case Wm.KeyUp:
                switch ( (Vk)m.WParam )
                {
                case Vk.Control:
                    if ( (m.LParam.ToInt32() & (1 << 24)) == 0 )
                        modifier &= ~KeyModifiers.LeftControl;
                    else
                        modifier &= ~KeyModifiers.RightControl;
                    break;
                case Vk.Alt:
                    if ( (m.LParam.ToInt32() & (1 << 24)) == 0 )
                        modifier &= ~KeyModifiers.LeftAlt;
                    else
                        modifier &= ~KeyModifiers.RightAlt;
                    break;
                case Vk.Shift:
                    if ( (m.LParam.ToInt32() & (1 << 24)) == 0 )
                        modifier &= ~KeyModifiers.LeftShift;
                    else
                        modifier &= ~KeyModifiers.RightShift;
                    break;
                }
                return true;

            case Wm.Char:
                //
                char c = (char)m.WParam;
                if ( c < (char)0x20
                    && !( TabEnabled && c == '\t' )
                    && !( MultilineEnabled && ( c == '\r' || c == '\n' ) )
                    && !( BackspaceEnabled && c == '\b' ) )
                    break;

                if ( c == '\r' )
                    c = '\n';   // Note: Control+ENTER will send \n, just ENTER will send \r

                if ( c == '\b' && textData.Length > 0 && textData[textData.Length - 1] != '\b' )
                {
                    // Backspace
                    textData.Length--;
                }
                else
                    textData.Append( c );

                OnTextChanged();
                return true;

            case Wm.Active:
                if ( ((int)m.WParam & 0xffff) == (int)Wa.Inactive )
                {
                    modifier = KeyModifiers.None;
                }
                break;  // Must not filter
            }
            return false;
        }

        #endregion
    }
#endif
}
