using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Graphics;
using System;

namespace OpenNefia.Core.UI
{
    public interface IInputHandler : IKeyBinder, IMouseBinder, IMouseMovedBinder, ITextInputBinder, IInputForwarder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="is_repeat"></param>
        void ReceiveKeyPressed(KeyPressedEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        void ReceiveKeyReleased(KeyPressedEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        void ReceiveTextInput(TextInputEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="isTouch"></param>
        void ReceiveMouseMoved(MouseMovedEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="button"></param>
        /// <param name="isTouch"></param>
        void ReceiveMousePressed(MousePressedEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="button"></param>
        /// <param name="isTouch"></param>
        void ReceiveMouseReleased(MousePressedEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        void HaltInput();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        bool IsModifierHeld(Keys modifier);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void UpdateKeyRepeats(float dt);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        bool RunKeyAction(Keys key, KeyPressState state);

        /// <summary>
        ///
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool RunTextInputAction(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        void ReleaseKey(Keys key, bool runEvents = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        void ReleaseMouseButton(MousePressedEventArgs args, bool runEvents = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        bool RunMouseMovedAction(MouseMovedEventArgs args);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        bool RunMouseAction(MousePressedEventArgs args);

        /// <summary>
        /// Run key actions based on the current state of the key handler.
        /// </summary>
        /// <param name="dt">Frame delta time.</param>
        void RunKeyActions(float dt);
    }
}
