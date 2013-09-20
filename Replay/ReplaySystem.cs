using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Kinect.Toolbox.Record {
  class ReplaySystem<T> where T : ReplayFrame, new() {
    internal List<T> Frames {
      get {
        return frames;
      }
    }

    internal event Action<T> FrameReady;
    readonly List<T> frames = new List<T>(); // Equivalent to ArrayList.
    int frameIndex = 0;
    CancellationTokenSource cancellationTokenSource;
    DispatcherTimer timer;

    public bool IsFinished {
      get;
      private set;
    }

    internal void AddFrame(BinaryReader reader) {
      T frame = new T();

      frame.CreateFromReader(reader);

      frames.Add(frame);
    }
    
    /// <summary>
    /// Starts replaying the frames either according to the provided timer or according to the 
    /// recorded timestamps.
    /// </summary>
    /// <param name="timer"></param>
    public void Start(DispatcherTimer timer) {
      if (timer == null) {
        Start();
      } else {
        Stop();
        this.timer = timer;
        timer.Tick += new EventHandler(OnTimerTick);
        IsFinished = false;
        timer.Start();
      }
    }

    public void Start() {
      Stop();

      IsFinished = false;

      cancellationTokenSource = new CancellationTokenSource();

      CancellationToken token = cancellationTokenSource.Token;

      Task.Factory.StartNew(() => {
        foreach (T frame in frames) {

          Thread.Sleep(TimeSpan.FromMilliseconds(frame.TimeStamp));

          if (token.IsCancellationRequested)
            break;

          if (FrameReady != null)
            FrameReady(frame);
        }

        IsFinished = true;
      }, token);
    }

    public void Stop() {
      if (cancellationTokenSource != null)
        cancellationTokenSource.Cancel();

      if (timer != null)
        timer.Stop();
    }

    private void OnTimerTick(object sender, EventArgs e) {
      if (frameIndex < frames.Count && FrameReady != null) {
        FrameReady(frames[frameIndex]);
        frameIndex++;
      } else {
        IsFinished = true;
        Stop();
      }
    }
  }
}
