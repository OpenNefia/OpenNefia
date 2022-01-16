using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public abstract record UiResult<T> where T: class
    {
        public bool HasValue { get => this is Finished; }

        [Obsolete("Make nullable")]
        public T Value
        {
            get
            {
                if (this is Finished)
                {
                    return (this as Finished)!.InnerValue;
                }
                else
                {
                    throw new Exception($"Tried to unwrap value on non-resultful UiResult: {this.GetType()}");
                }
            }
        }

        public sealed record Finished(T InnerValue) : UiResult<T>
        {
            public override string ToString() => $"Finished({InnerValue})";
        }
        public sealed record Cancelled() : UiResult<T>
        {
            public override string ToString() => $"Cancelled()";
        }
        public sealed record Error(Exception Exception) : UiResult<T>
        {
            public override string ToString() => $"Error({Exception.Message})";
        }
    }
}
