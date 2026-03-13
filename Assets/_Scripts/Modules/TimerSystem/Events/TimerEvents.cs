namespace Modules.TimerSystem.Events
{
    public struct TimerTickEvent
    {
        public long CurrentTime { get; }
        public TimerTickEvent(long currentTime)
        {
            CurrentTime = currentTime;
        }
    }
}