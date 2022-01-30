using System;

namespace OpenNefia.Core.Rendering
{
    public class FrameCounter
    {
        public float FrameDelaySecs;
        public uint MaxFrames;

        private float Dt = 0f;
        public float Frame { get; private set; } = 0f;
        public uint FrameInt { get => (uint)Frame; }
        public bool IsFinished { get => Frame >= MaxFrames; }

        public FrameCounter(float delaySecs, uint maxFrames)
        {
            FrameDelaySecs = delaySecs;
            MaxFrames = maxFrames;
        }
        
        public void Update(float dt)
        {
            Dt += dt;
            Frame = Math.Clamp((Dt / FrameDelaySecs), 0f, (float)MaxFrames);
        }
    }
}
