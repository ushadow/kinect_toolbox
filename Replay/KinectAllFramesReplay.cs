using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Kinect.Toolbox.Record {
  /// <summary>
  /// Replays frames from all three streams synchronously.
  /// </summary>
  public class KinectAllFramesReplay : IDisposable {

    readonly SynchronizationContext synchronizationContext;

    public event EventHandler<ReplayAllFramesReadyEventArgs> AllFramesReady;

    public CoordinateMapper CoordinateMapper { get; private set; }

    public int FrameCount {
      get {
        if (allFramesReplay == null)
          return 0;
        else
          return allFramesReplay.Frames.Count;
      }
    }

    public bool Started { get; internal set; }

    public bool IsFinished {
      get {
        if (allFramesReplay != null && !allFramesReplay.IsFinished)
          return false;

        return true;
      }
    }

    Stream stream;
    BinaryReader reader;

    ReplaySystem<ReplayAllFrames> allFramesReplay = new ReplaySystem<ReplayAllFrames>();

    public KinectAllFramesReplay(Stream stream) {
      this.stream = stream;
      reader = new BinaryReader(stream);

      CoordinateMapper = ReadCoordinateMapperParams();
      synchronizationContext = SynchronizationContext.Current;
      reader.ReadInt32();
      while (reader.BaseStream.Position != reader.BaseStream.Length) {
        allFramesReplay.AddFrame(reader);
      }
    }

    public ReplayAllFrames FrameAt(int index) {
      if (index < FrameCount)
        return allFramesReplay.Frames[index];
      return null;
    }

    public void Start(DispatcherTimer timer = null) {
      if (Started)
        throw new Exception("KinectReplay already started");

      Started = true;

      if (allFramesReplay != null) {
        allFramesReplay.FrameReady += frame => synchronizationContext.Send(state => {
          if (AllFramesReady != null)
            AllFramesReady(this, new ReplayAllFramesReadyEventArgs { AllFrames = frame });
        }, null);
        allFramesReplay.Start(timer);
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
        reader.Close();
        reader = null;
        stream = null;
      }
    }

    CoordinateMapper ReadCoordinateMapperParams() {
      int count = reader.ReadInt32();
      var coordinateParams = reader.ReadBytes(count);
      return new CoordinateMapper(coordinateParams);
    }
  }
}
