using Kinect.Toolbox.Record;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect.Toolbox.Record {
  public class ReplayAllFramesReadyEventArgs : EventArgs {
    public ReplayAllFrames AllFrames { get; set; }
    public ReplayColorImageFrame ColorImageFrame {
      get {
        return AllFrames.ColorImageFrame;
      }
    }

    public ReplayDepthImageFrame DepthImageFrame {
      get {
        return AllFrames.DepthImageFrame;
      }
    }

    public ReplaySkeletonFrame SkeletonFrame {
      get {
        return AllFrames.SkeletonFrame;
      }
    }
  }

  public class ReplayAllFrames : ReplayFrame {
    
    public ReplayColorImageFrame ColorImageFrame { get; private set; }
    public ReplayDepthImageFrame DepthImageFrame { get; private set; }
    public ReplaySkeletonFrame SkeletonFrame { get; private set; }

    public ReplayAllFrames() {
      SkeletonFrame  = new ReplaySkeletonFrame();
      DepthImageFrame = new ReplayDepthImageFrame();
      ColorImageFrame = new ReplayColorImageFrame();
    }

    internal override void CreateFromReader(BinaryReader reader) {
      for (Int32 i = 0; i < 3; i++) {
        KinectRecordOptions header = (KinectRecordOptions)reader.ReadInt32();
        switch (header) {
          case KinectRecordOptions.Skeletons:
            SkeletonFrame.CreateFromReader(reader);
            break;
          case KinectRecordOptions.Depth:
            DepthImageFrame.CreateFromReader(reader);
            TimeStamp = DepthImageFrame.TimeStamp;
            FrameNumber = DepthImageFrame.FrameNumber;
            break;
          case KinectRecordOptions.Color:
            ColorImageFrame.CreateFromReader(reader);
            return;
        }
      }
    }

  }
}
