using System;

namespace OpenNefia.Core.Rendering
{
    public class FrameCounter
    {
        public float FrameDelayMillis;
        public uint MaxFrames;

        private float Dt = 0f;
        public float Frame { get; private set; } = 0f;
        public uint FrameInt { get => (uint)Frame; }
        public bool IsFinished { get => Frame >= MaxFrames; }

        public FrameCounter(float delayMillis, uint maxFrames)
        {
            FrameDelayMillis = delayMillis;
            MaxFrames = maxFrames;
        }
        
        public void Update(float dt)
        {
            Dt += dt * 1000f;
            Frame = Math.Clamp((Dt / FrameDelayMillis), 0f, (float)MaxFrames);
        }
    }
}
