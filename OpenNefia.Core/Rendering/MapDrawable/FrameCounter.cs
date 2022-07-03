using System;

namespace OpenNefia.Core.Rendering
{
    public class FrameCounter
    {
        public float FrameDelaySecs { get; }
        public int MaxFrames { get; }

        private float Dt = 0f;
        public float Frame { get; private set; } = 0f;
        public int FrameInt { get => (int)Frame; }
        public bool IsFinished { get => Frame >= MaxFrames; }

        public FrameCounter(float delaySecs, int maxFrames)
        {
            FrameDelaySecs = delaySecs;
            MaxFrames = Math.Max(maxFrames, 0);
        }

        public FrameCounter(float delaySecs, uint maxFrames) 
            : this(delaySecs, (int)maxFrames) { }

        public void Update(float dt)
        {
            Dt += dt;
            Frame = Math.Clamp((Dt / FrameDelaySecs), 0f, MaxFrames);
        }
    }
}
