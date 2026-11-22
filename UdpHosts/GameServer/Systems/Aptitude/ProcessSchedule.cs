namespace GameServer.Systems.Aptitude;

public static class ProcessSchedule
{
    public static bool Due(Context context, uint period, uint seedId)
    {
        var now = context.CurrentTime;
        if (context.ProcessBase == null)
        {
            var initTime = context.InitTime ?? now;
            var self = context.Self?.AeroEntityId.Backing ?? 0;
            var seed = (uint)(self >> 32) ^ seedId ^ (uint)self;
            var processBase = unchecked(initTime + (PRNG.PRNG.UInt(seed) % period) - period);
            context.ProcessBase = processBase;
            context.NextProcessTime = unchecked(now - ((now - processBase) % period) + period);
        }

        if (unchecked((int)(now - context.NextProcessTime)) < 0)
        {
            return false;
        }

        var time = unchecked(now - ((now - context.ProcessBase.Value) % period));
        context.CurrentTime = time;
        context.NextProcessTime = unchecked(time + period);
        return true;
    }
}
