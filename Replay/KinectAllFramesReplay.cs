using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kinect.Toolbox.Record {
  public class KinectAllFramesReplay : IDisposable {

    public event EventHandler<ReplayAllFramesReadyEventArgs> AllFramesReady;

    Stream stream;
    BinaryReader reader;
    readonly SynchronizationContext synchronizationContext;

    ReplaySystem<ReplayAllFrames> allFramesReplay = new ReplaySystem<ReplayAllFrames>();

    public bool Started { get; internal set; }

    public bool IsFinished {
      get {
        if (allFramesReplay != null && !allFramesReplay.IsFinished)
          return false;

        return true;
      }
    }

    public KinectAllFramesReplay(Stream stream) {
      this.stream = stream;
      reader = new BinaryReader(stream);
      synchronizationContext = SynchronizationContext.Current;
      reader.ReadInt32();
      while (reader.BaseStream.Position != reader.BaseStream.Length) {
        allFramesReplay.AddFrame(reader);
      }
    }

    public void Start() {
      if (Started)
        throw new Exception("KinectReplay already started");

      Started = true;

      if (allFramesReplay != null) {
        allFramesReplay.Start();
        allFramesReplay.FrameReady += frame => synchronizationContext.Send(state => {
          if (AllFramesReady != null)
            AllFramesReady(this, new ReplayAllFramesReadyEventArgs { AllFrames = frame });
        }, null);
      }
    }

    public void Stop() {
      if (allFramesReplay != null) {
        allFramesReplay.Stop();
      }

      Started = false;
    }

    public void Dispose() {
      Stop();

      allFramesReplay = null;

      if (reader != null) {
        reader.Dispose();
        reader = null;
      }

      if (stream != null) {
        stream.Dispose();
        stream = null;
      }
    }
  }
}
