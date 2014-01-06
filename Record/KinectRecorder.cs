using System;
using System.IO;

using Microsoft.Kinect;
using System.Collections.ObjectModel;

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
    public KinectRecorder(KinectRecordOptions options, CoordinateMapper mapper,
        float colorFocalLength, float depthFocalLength, Stream stream) {
      Options = options;

      recordStream = stream;
      writer = new BinaryWriter(recordStream);

      var coordParams = mapper.ColorToDepthRelationalParameters;
      int count = coordParams.Count;
      byte[] array = new byte[count];
      coordParams.CopyTo(array, 0);
      writer.Write(count);
      writer.Write(array);
      writer.Write(colorFocalLength);
      writer.Write(depthFocalLength);

      writer.Write((int)Options);

      if ((Options & KinectRecordOptions.Color) != 0) {
        colorRecoder = new ColorRecorder(writer);
      }
      if ((Options & KinectRecordOptions.Depth) != 0) {
        depthRecorder = new DepthRecorder(writer);
      }
      if ((Options & KinectRecordOptions.Skeletons) != 0) {
        skeletonRecorder = new SkeletonRecorder(writer);
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
    public void Record(SkeletonFrame sf, DepthImageFrame df, ColorImageFrame cf) {
      lock (this) {
        if (sf != null)
          Record(sf);
        if (df != null)
          Record(df);
        if (cf != null)
          Record(cf);
      }
    }

    public void Record(SkeletonFrame frame) {
      if (writer == null)
        throw new Exception("This recorder is stopped");

      if (skeletonRecorder == null)
        throw new Exception("Skeleton recording is not actived on this KinectRecorder");

      skeletonRecorder.Record(frame);
      Flush();
    }

    public void Record(ColorImageFrame frame) {
      if (writer == null)
        throw new Exception("This recorder is stopped");

      if (colorRecoder == null)
        throw new Exception("Color recording is not actived on this KinectRecorder");

      colorRecoder.Record(frame);
      Flush();
    }

    public void Record(DepthImageFrame frame) {
      if (writer == null)
        throw new Exception("This recorder is stopped");

      if (depthRecorder == null)
        throw new Exception("Depth recording is not actived on this KinectRecorder");

      depthRecorder.Record(frame);
      Flush();
    }

    void Flush() {
      var now = DateTime.Now;

      if (now.Subtract(previousFlushDate).TotalSeconds > 60) {
        previousFlushDate = now;
        writer.Flush();
      }
    }

    /// <summary>
    /// Closes the recording stream.
    /// </summary>
    public void Close() {
      if (writer == null)
        throw new Exception("This recorder is already stopped");

      lock (this) {
        writer.Close();
        recordStream.Close();
        recordStream = null;
      }
    }
  }
}
