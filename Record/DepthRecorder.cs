using System;
using System.IO;
using Microsoft.Kinect;

using Common.Logging;

namespace Kinect.Toolbox.Record {
  class DepthRecorder {
    static readonly ILog Log = LogManager.GetCurrentClassLogger();
    DateTime referenceTime;
    readonly BinaryWriter writer;

    internal DepthRecorder(BinaryWriter writer, DateTime refTime) {
      this.writer = writer;
      referenceTime = refTime;
    }

    public void Record(DepthImageFrame frame, DateTime time) {
      // Header
      writer.Write((int)KinectRecordOptions.Depth);

      // Data
      TimeSpan timeSpan = time.Subtract(referenceTime);
      referenceTime = time;
      writer.Write((long)timeSpan.TotalMilliseconds);
      writer.Write(frame.BytesPerPixel);
      writer.Write((int)frame.Format);
      writer.Write(frame.Width);
      writer.Write(frame.Height);

      writer.Write(frame.FrameNumber);

      // Bytes
      short[] shorts = new short[frame.PixelDataLength];
      frame.CopyPixelDataTo(shorts);
      writer.Write(shorts.Length);
      foreach (short s in shorts) {
        writer.Write(s);
      }
    }
  }
}
