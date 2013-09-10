using System;
using System.IO;
using Microsoft.Kinect;

namespace Kinect.Toolbox.Record {
  public class KinectRecorder {
    Stream recordStream;
    readonly BinaryWriter writer;

    DateTime previousFlushDate;

    // Recorders
    readonly ColorRecorder colorRecoder;
    readonly DepthRecorder depthRecorder;
    readonly SkeletonRecorder skeletonRecorder;

    public KinectRecordOptions Options { get; set; }

    // Ctr
    public KinectRecorder(KinectRecordOptions options, Stream stream) {
      Options = options;

      recordStream = stream;
      writer = new BinaryWriter(recordStream);

      writer.Write((int)Options);

      var refTime = DateTime.Now;

      if ((Options & KinectRecordOptions.Color) != 0) {
        colorRecoder = new ColorRecorder(writer, refTime);
      }
      if ((Options & KinectRecordOptions.Depth) != 0) {
        depthRecorder = new DepthRecorder(writer, refTime);
      }
      if ((Options & KinectRecordOptions.Skeletons) != 0) {
        skeletonRecorder = new SkeletonRecorder(writer, refTime);
      }

      previousFlushDate = DateTime.Now;
    }

    /// <summary>
    /// Records frames from all three streams at the same time.
    /// </summary>
    /// <param name="sf"></param>
    /// <param name="df"></param>
    /// <param name="cf"></param>
    /// <param name="time"></param>
    public void Record(SkeletonFrame sf, DepthImageFrame df, ColorImageFrame cf, DateTime time) {
      if (sf != null)
        Record(sf, time);
      if (df != null)
        Record(df, time);
      if (cf != null)
        Record(cf, time);
    }

    public void Record(SkeletonFrame frame, DateTime time) {
      if (writer == null)
        throw new Exception("This recorder is stopped");

      if (skeletonRecorder == null)
        throw new Exception("Skeleton recording is not actived on this KinectRecorder");

      skeletonRecorder.Record(frame, time);
      Flush();
    }

    public void Record(ColorImageFrame frame, DateTime time) {
      if (writer == null)
        throw new Exception("This recorder is stopped");

      if (colorRecoder == null)
        throw new Exception("Color recording is not actived on this KinectRecorder");

      colorRecoder.Record(frame, time);
      Flush();
    }

    public void Record(DepthImageFrame frame, DateTime time) {
      if (writer == null)
        throw new Exception("This recorder is stopped");

      if (depthRecorder == null)
        throw new Exception("Depth recording is not actived on this KinectRecorder");

      depthRecorder.Record(frame, time);
      Flush();
    }

    void Flush() {
      var now = DateTime.Now;

      if (now.Subtract(previousFlushDate).TotalSeconds > 60) {
        previousFlushDate = now;
        writer.Flush();
      }
    }

    public void Stop() {
      if (writer == null)
        throw new Exception("This recorder is already stopped");

      writer.Close();
      writer.Dispose();

      recordStream.Dispose();
      recordStream = null;
    }
  }
}
