using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Kinect.Toolbox.Record;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Kinect;

namespace Kinect.Toolbox
{
    public class SkeletonDisplayManager
    {
        readonly Canvas rootCanvas;
        readonly CoordinateMapper mapper;

        public SkeletonDisplayManager(CoordinateMapper mapper, Canvas root)
        {
            rootCanvas = root;
            this.mapper = mapper;
        }

        void GetCoordinates(JointType jointType, IEnumerable<Joint> joints, out float x, 
            out float y, Object format) {
            var joint = joints.First(j => j.JointType == jointType);

            Vector2 vector2 = Tools.Convert(mapper, joint.Position, format);

            x = (float)(vector2.X * rootCanvas.ActualWidth);
            y = (float)(vector2.Y * rootCanvas.ActualHeight);
        }

        void Plot(JointType centerID, IEnumerable<Joint> joints, Object format) {
            float centerX;
            float centerY;

            GetCoordinates(centerID, joints, out centerX, out centerY, format);

            const double diameter = 8;

            Ellipse ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 4.0,
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeLineJoin = PenLineJoin.Round
            };

            Canvas.SetLeft(ellipse, centerX - ellipse.Width / 2);
            Canvas.SetTop(ellipse, centerY - ellipse.Height / 2);

            rootCanvas.Children.Add(ellipse);
        }

        void Plot(JointType centerID, JointType baseID, JointCollection joints, Object format)
        {
            float centerX;
            float centerY;

            GetCoordinates(centerID, joints, out centerX, out centerY, format);

            float baseX;
            float baseY;

            GetCoordinates(baseID, joints, out baseX, out baseY, format);

            double diameter = Math.Abs(baseY - centerY);

            Ellipse ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 4.0,
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeLineJoin = PenLineJoin.Round
            };

            Canvas.SetLeft(ellipse, centerX - ellipse.Width / 2);
            Canvas.SetTop(ellipse, centerY - ellipse.Height / 2);

            rootCanvas.Children.Add(ellipse);
        }

        void Trace(JointType sourceID, JointType destinationID, JointCollection joints, 
                   Object format) {
            float sourceX;
            float sourceY;

            GetCoordinates(sourceID, joints, out sourceX, out sourceY, format);

            float destinationX;
            float destinationY;

            GetCoordinates(destinationID, joints, out destinationX, out destinationY, format);

            Line line = new Line
                            {
                                X1 = sourceX,
                                Y1 = sourceY,
                                X2 = destinationX,
                                Y2 = destinationY,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                StrokeThickness = 4.0,                                
                                Stroke = new SolidColorBrush(Colors.Green),
                                StrokeLineJoin = PenLineJoin.Round
                            };


            rootCanvas.Children.Add(line);
        }

        public void Draw(Skeleton[] skeletons, bool seated, Object format) {
            rootCanvas.Children.Clear();
            foreach (Skeleton skeleton in skeletons) {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                Plot(JointType.HandLeft, skeleton.Joints, format);
                Trace(JointType.HandLeft, JointType.WristLeft, skeleton.Joints, format);
                Plot(JointType.WristLeft, skeleton.Joints, format);
                Trace(JointType.WristLeft, JointType.ElbowLeft, skeleton.Joints, format);
                Plot(JointType.ElbowLeft, skeleton.Joints, format);
                Trace(JointType.ElbowLeft, JointType.ShoulderLeft, skeleton.Joints, format);
                Plot(JointType.ShoulderLeft, skeleton.Joints, format);
                Trace(JointType.ShoulderLeft, JointType.ShoulderCenter, skeleton.Joints, format);
                Plot(JointType.ShoulderCenter, skeleton.Joints, format);

                Trace(JointType.ShoulderCenter, JointType.Head, skeleton.Joints, format);

                Plot(JointType.Head, JointType.ShoulderCenter, skeleton.Joints, format);

                Trace(JointType.ShoulderCenter, JointType.ShoulderRight, skeleton.Joints, format);
                Plot(JointType.ShoulderRight, skeleton.Joints, format);
                Trace(JointType.ShoulderRight, JointType.ElbowRight, skeleton.Joints, format);
                Plot(JointType.ElbowRight, skeleton.Joints, format);
                Trace(JointType.ElbowRight, JointType.WristRight, skeleton.Joints, format);
                Plot(JointType.WristRight, skeleton.Joints, format);
                Trace(JointType.WristRight, JointType.HandRight, skeleton.Joints, format);
                Plot(JointType.HandRight, skeleton.Joints, format);

                if (!seated) {
                    Trace(JointType.ShoulderCenter, JointType.Spine, skeleton.Joints, format);
                    Plot(JointType.Spine, skeleton.Joints, format);
                    Trace(JointType.Spine, JointType.HipCenter, skeleton.Joints, format);
                    Plot(JointType.HipCenter, skeleton.Joints, format);

                    Trace(JointType.HipCenter, JointType.HipLeft, skeleton.Joints, format);
                    Plot(JointType.HipLeft, skeleton.Joints, format);
                    Trace(JointType.HipLeft, JointType.KneeLeft, skeleton.Joints, format);
                    Plot(JointType.KneeLeft, skeleton.Joints, format);
                    Trace(JointType.KneeLeft, JointType.AnkleLeft, skeleton.Joints, format);
                    Plot(JointType.AnkleLeft, skeleton.Joints, format);
                    Trace(JointType.AnkleLeft, JointType.FootLeft, skeleton.Joints, format);
                    Plot(JointType.FootLeft, skeleton.Joints, format);

                    Trace(JointType.HipCenter, JointType.HipRight, skeleton.Joints, format);
                    Plot(JointType.HipRight, skeleton.Joints, format);
                    Trace(JointType.HipRight, JointType.KneeRight, skeleton.Joints, format);
                    Plot(JointType.KneeRight, skeleton.Joints, format);
                    Trace(JointType.KneeRight, JointType.AnkleRight, skeleton.Joints, format);
                    Plot(JointType.AnkleRight, skeleton.Joints, format);
                    Trace(JointType.AnkleRight, JointType.FootRight, skeleton.Joints, format);
                    Plot(JointType.FootRight, skeleton.Joints, format);
                }
            }
        }
    }
}
