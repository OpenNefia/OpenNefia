using System;

namespace OpenNefia.Core.Rendering
{
    public class FrameCounter
    {
        public float FrameDelaySecs { get; }
        public int MaxFrames { get; }

        private float Dt = 0f;
        public float Frame { get; private set; } = 0f;
        public float LastFramesPassed { get; private set; } = 0f;
        public int FrameInt { get => (int)Frame; }
        public bool IsFinished { get => Frame >= MaxFrames; }

        public FrameCounter() : this(0.5f)
        {
        }

        public FrameCounter(float delaySecs, int maxFrames = int.MaxValue)
        {
            FrameDelaySecs = delaySecs;
            MaxFrames = Math.Max(maxFrames, 0);
        }

        public FrameCounter(float delaySecs, uint maxFrames)
            : this(delaySecs, (int)maxFrames) { }

        public void Update(float dt)
        {
            Dt += dt;

            var newFrame = Dt / FrameDelaySecs;
            LastFramesPassed = newFrame - Frame;
            Frame = float.Clamp(newFrame, 0f, MaxFrames);
        }
    }
}
