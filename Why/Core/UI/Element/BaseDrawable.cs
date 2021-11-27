using Love;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Element
{
    public abstract class BaseDrawable : IDrawable
    {
        public int Width { get; private set; } = 0;
        public int Height { get; private set; } = 0;
        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;

        public void SetSizeAndPosition(Rectangle rect)
        {
            this.SetSize(rect.Width, rect.Height);
            this.SetPosition(rect.X, rect.Y);
        }

        public virtual void SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public virtual void SetPosition(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public bool ContainsPoint(int x, int y)
        {
            return this.X <= x && this.Y <= y && this.X + this.Width > x && this.Y + this.Height > y;
        }

        public abstract void Update(float dt);
        public abstract void Draw();

        public virtual void Dispose()
        {
        }
    }
}
