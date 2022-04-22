using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SimpleCpuMeter
{
    /// <summary>
    /// Records cpu data measurements and produces basic collection of percentage used per time frame given.
    /// <para>Use Start() / Stop() methods to run a recording session.</para>
    /// <para>See Get methods for collected data aggregation.</para>
    /// </summary>
    public class CpuMeter
    {
        public CpuMeter() : this(1000) { }
        public CpuMeter(int Interval = 1000)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _delay = Interval;
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            else IncompatibleOS();
        }
        
        private void IncompatibleOS() => throw new InvalidOperationException("Class only supported for Windows operating system.");
        private readonly dynamic? cpuCounter;
        private readonly List<KeyValuePair<int, float>> measurements = new();
        private bool isRunning;
        private int _delay, _runtimetotal;
        /// <summary>
        /// Collection of cpu usage snapshots per each interval in milliseconds.
        /// </summary>
        /// <returns><see cref="int"/> milliseconds current time and <see cref="float"/> percentage cpu at that point.</returns>
        public IEnumerable<KeyValuePair<int, float>> Get() => measurements;
        /// <summary>
        /// Function to average the usage of cpu during the measured time.
        /// </summary>
        /// <returns><see cref="float"/>? 0-100</returns>
        public float? GetAverage() => Get().Average(x => x.Value);
        /// <summary>
        /// Function to average the usage of cpu during the measured time.
        /// </summary>
        /// <returns><see cref="float"/>? 0-100</returns>
        public float? GetMaxValue() => Get().Max(x => x.Value);
        /// <summary>
        /// Function to average the usage of cpu during the measured time.
        /// </summary>
        /// <returns><see cref="float"/>? 0-100</returns>
        public float? GetMinValue() => Get().Min(x => x.Value);
        /// <summary>
        /// Total runtime of the meter.
        /// </summary>
        /// <returns></returns>
        public int? GetRuntime() => _runtimetotal;
        /// <summary>
        /// Total amount of snapshots for the duration of the meter.
        /// </summary>
        /// <returns></returns>
        public int? GetSnapshotsCount() => _runtimetotal / _delay;
        //best to reinstantiate class with new interval in constructor.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int SetInterval(int interval) => _delay = interval;
        /// <summary>
        /// Starts recording of cpu usage.
        /// </summary>
        public async void Start()
        {
            _runtimetotal = 0;
            isRunning = true;
            await AddToMeasurements();
        }
        /// <summary>
        /// Stops the recording of cpu usage.
        /// </summary>
        public void Stop() => isRunning = false;
        protected async Task<int> AddToMeasurements()
        {
            while (isRunning)
            {
                measurements.Add(new(_runtimetotal, cpuCounter!.NextValue()));
                _runtimetotal += _delay;
                await Task.Delay(_delay);
            }
            return 0;
        }
    }
}
