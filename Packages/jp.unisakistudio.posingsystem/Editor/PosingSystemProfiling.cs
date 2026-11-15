using System;
using UnityEngine.Profiling;

namespace jp.unisakistudio.posingsystemeditor
{
    // Named samples for Unity's Profiler around the slow phases of a prebuild. They cost nothing
    // measurable while the Profiler is not recording and make a regression visible without a
    // deep profile (which runs out of memory on a full prebuild).
    internal static class PosingSystemProfiling
    {
        internal readonly struct Scope : IDisposable
        {
            public void Dispose() => Profiler.EndSample();
        }

        internal static Scope Sample(string name)
        {
            Profiler.BeginSample(name);
            return default;
        }
    }
}
