using System;
using System.Runtime.InteropServices;

namespace GameServer;

/// <summary>
///     Windows wakes waiting threads every 15.6 ms by default, too coarse for the 5 ms shard tick. timeBeginPeriod(1)
///     fixes that, but Windows 11 ignores it for a process without a visible window (service, Docker, Aspire) unless
///     the process opts out of power throttling first.
/// </summary>
public static class TimerResolution
{
    private const int ProcessPowerThrottling = 4;
    private const uint PowerThrottlingCurrentVersion = 1;
    private const uint PowerThrottlingIgnoreTimerResolution = 0x4;

    public static bool Raise()
    {
        if (!OperatingSystem.IsWindows())
        {
            return true;
        }

        var state = new PowerThrottlingState
        {
            Version = PowerThrottlingCurrentVersion,
            ControlMask = PowerThrottlingIgnoreTimerResolution,
            StateMask = 0,
        };

        // Fails on Windows before 11, where the timer resolution request is always honored
        _ = SetProcessInformation(GetCurrentProcess(), ProcessPowerThrottling, ref state, (uint)Marshal.SizeOf<PowerThrottlingState>());

        return TimeBeginPeriod(1) == 0;
    }

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    private static extern uint TimeBeginPeriod(uint milliseconds);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProcessInformation(IntPtr process, int informationClass, ref PowerThrottlingState information, uint size);

    [StructLayout(LayoutKind.Sequential)]
    private struct PowerThrottlingState
    {
        public uint Version;
        public uint ControlMask;
        public uint StateMask;
    }
}
