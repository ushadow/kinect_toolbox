using System;
using System.IO;
using Microsoft.Kinect;
using System.Runtime.Serialization.Formatters.Binary;

using Common.Logging;

namespace Kinect.Toolbox.Record {
  class SkeletonRecorder {
    static readonly ILog Log = LogManager.GetCurrentClassLogger();
    DateTime referenceTime;
    readonly BinaryWriter writer;

    internal SkeletonRecorder(BinaryWriter writer, DateTime refTime) {
      this.writer = writer;
      referenceTime = refTime;
    }

    public void Record(SkeletonFrame frame, DateTime time) {
      // Header
      writer.Write((int)KinectRecordOptions.Skeletons);

      // Data
      TimeSpan timeSpan = time.Subtract(referenceTime);
      referenceTime = time;
      writer.Write((long)timeSpan.TotalMilliseconds);
      writer.Write((int)frame.TrackingMode);
      writer.Write(frame.FloorClipPlane.Item1);
      writer.Write(frame.FloorClipPlane.Item2);
      writer.Write(frame.FloorClipPlane.Item3);
      writer.Write(frame.FloorClipPlane.Item4);

      writer.Write(frame.FrameNumber);

      // Skeletons
      Skeleton[] skeletons = frame.GetSkeletons();
      frame.CopySkeletonDataTo(skeletons);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(writer.BaseStream, skeletons);
    }
  }
}
