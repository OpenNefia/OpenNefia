using System.Runtime.InteropServices;

public static class VolumeHelper
{
    [DllImport("Winmm.dll")]
    private static extern int midiOutSetVolume(int hwo, System.UInt32 pdwVolume);


    [DllImport("Winmm.dll")]
    private static extern uint midiOutGetVolume(int hwo, out System.UInt32 pdwVolume);
    public static double CurrentVolume = 50;
    public static void SetVolume(double volumeSize)
    {
        if (volumeSize < 0)
            volumeSize = 0;
        if (volumeSize > 100)
            volumeSize = 100;
        System.UInt32 Value = (System.UInt32)((double)0xffff * (double)volumeSize / 100.0);
        if (Value < 0)
            Value = 0;
        if (Value > 0xffff)
            Value = 0xffff;
        System.UInt32 left = (System.UInt32)Value;
        System.UInt32 right = (System.UInt32)Value;
        midiOutSetVolume(0, left << 16 | right); 
        CurrentVolume = volumeSize;
    }
    public static int GetVolume()
    {
        System.UInt32 value = 0;
        midiOutSetVolume(0, value);
        return (int)(value >> 16);
    }
}