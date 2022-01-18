﻿using OpenNefia.Core.Maths;
using System;

namespace OpenNefia.Core.UI.Element
{
    public interface IDrawable : IDisposable
    {
        public Vector2 Size { get; }

        /// <summary>
        ///     The size of this drawable, in physical pixels.
        /// </summary>
        public Vector2i PixelSize => (Vector2i)(Size * UIScale);

        /// <summary>
        ///     The position of the top left corner of the control, in virtual pixels.
        ///     Unlike Robust, this is relative to the global viewport.
        /// </summary>
        /// <seealso cref="PixelPosition"/>
        /// <seealso cref="GlobalPosition"/>
        public Vector2 Position { get; }

        /// <summary>
        ///     Absolute position of the drawable element.
        ///     Unlike Robust, this is relative to the global viewport.
        /// </summary>
        public Vector2i PixelPosition { get => PixelRect.TopLeft; }

        /// <summary>
        ///     Represents the "rectangle" of the control relative to the global viewport, in virtual pixels.
        /// </summary>
        /// <seealso cref="PixelRect"/>
        public UIBox2 Rect => UIBox2.FromDimensions(Position, Size);

        /// <summary>
        ///     Represents the "rectangle" of the control relative to the global viewport, in physical pixels.
        /// </summary>
        /// <seealso cref="Rect"/>
        public UIBox2i PixelRect => UIBox2i.FromDimensions(PixelPosition, PixelSize);

        /// <summary>
        ///     A <see cref="UIBox2"/> with the top left at 0,0 and the size equal to <see cref="Size"/>.
        /// </summary>
        /// <seealso cref="PixelSizeBox"/>
        public UIBox2 SizeBox => new(Vector2.Zero, Size);

        /// <summary>
        ///     A <see cref="UIBox2i"/> with the top left at 0,0 and the size equal to <see cref="PixelSize"/>.
        /// </summary>
        /// <seealso cref="SizeBox"/>
        public UIBox2i PixelSizeBox => new(Vector2i.Zero, PixelSize);

        /// <summary>
        ///     The width of the control, in virtual pixels.
        /// </summary>
        /// <seealso cref="PixelWidth"/>
        public float Width => Size.X;

        /// <summary>
        ///     The height of the control, in virtual pixels.
        /// </summary>
        /// <seealso cref="PixelHeight"/>
        public float Height => Size.Y;

        /// <summary>
        ///     The width of the control, in physical pixels.
        /// </summary>
        /// <seealso cref="Width"/>
        public int PixelWidth => PixelSize.X;

        /// <summary>
        ///     The height of the control, in physical pixels.
        /// </summary>
        /// <seealso cref="Height"/>
        public int PixelHeight => PixelSize.Y;

        /// <summary>
        ///     The X coordinate of the control, in virtual pixels.
        /// </summary>
        public float X => Position.X;

        /// <summary>
        ///     The Y coordinate of the control, in virtual pixels.
        /// </summary>
        public float Y => Position.Y;

        /// <summary>
        ///     The Y coordinate of the control, in physical pixels.
        /// </summary>
        public int PixelX => PixelPosition.X;

        /// <summary>
        ///     The Y coordinate of the control, in physical pixels.
        /// </summary>
        public int PixelY => PixelPosition.Y;

        /// <summary>
        ///     The amount of "real" pixels a virtual pixel takes up.
        ///     The higher the number, the bigger the interface.
        ///     I.e. UIScale units are real pixels (rp) / virtual pixels (vp),
        ///     real pixels varies depending on interface, virtual pixels doesn't.
        ///     And vp * UIScale = rp, and rp / UIScale = vp
        /// </summary>
        public virtual float UIScale => 1f;

        /// <summary>
        /// Sets the size of this component, in virtual pixels.
        /// </summary>
        void SetSize(float width, float height);

        /// <summary>
        /// Sets the position of this component, in virtual pixels.
        /// </summary>
        void SetPosition(float x, float y);

        bool ContainsPoint(Vector2 point);

        void Update(float dt);
        void Draw();
    }
}
