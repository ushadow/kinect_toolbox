﻿using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace Kinect.Toolbox.Record {
  /// <summary>
  /// Replays frames from all three streams synchronously. The recording must record all three
  /// streams.
  /// </summary>
  public class KinectAllFramesReplay : IDisposable {

    readonly SynchronizationContext synchronizationContext;

    public event EventHandler<ReplayAllFramesReadyEventArgs> AllFramesReady;

    public Byte[] KinectParams { get; private set; }
    public float ColorNominalFocalLengthInPixels { get; private set; }
    public float DepthNominalFocalLengthInPixels { get; private set; }

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
      synchronizationContext = SynchronizationContext.Current;
      
      this.stream = stream;
      reader = new BinaryReader(stream);

      KinectParams = ReadCoordinateMapperParams();
      ColorNominalFocalLengthInPixels = reader.ReadSingle();
      DepthNominalFocalLengthInPixels = reader.ReadSingle();
      
      var options = (KinectRecordOptions) reader.ReadInt32();

      if (!IsValidOptions(options)) {
        throw new Exception(String.Format("Not all streams are recorded. Record option = {0}", 
                                          options));
      }

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

    Byte[] ReadCoordinateMapperParams() {
      int count = reader.ReadInt32();
      return reader.ReadBytes(count);
    }

    bool IsValidOptions(KinectRecordOptions options) {
      return options.Equals(KinectRecordOptions.Color | KinectRecordOptions.Depth |
                            KinectRecordOptions.Skeletons);
    }
  }
}
