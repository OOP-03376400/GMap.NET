﻿
namespace GMap.NET.GtkSharp
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.Serialization;
    using System.Windows.Forms;
    using GMap.NET;    

    /// <summary>
    /// GMap.NET route
    /// </summary>
    [Serializable]
    public class GMapRoute : MapRoute, ISerializable, IDeserializationCallback, IDisposable
    {
        GMapOverlay overlay;
        public GMapOverlay Overlay
        {
            get
            {
                return overlay;
            }
            internal set
            {
                overlay = value;
            }
        }

        private bool visible = true;

        /// <summary>
        /// is marker visible
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return visible;
            }
            set
            {
                if (value != visible)
                {
                    visible = value;

                    if (Overlay != null && Overlay.Control != null)
                    {
                        if (visible)
                        {
                            Overlay.Control.UpdateRouteLocalPosition(this);
                        }
                        else
                        {
                            if (Overlay.Control.IsMouseOverRoute)
                            {
                                Overlay.Control.IsMouseOverRoute = false;
                                Overlay.Control.RestoreCursorOnLeave();
                            }
                        }

                        {
                            if (!Overlay.Control.HoldInvalidation)
                            {
                                Overlay.Control.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// can receive input
        /// </summary>
        public bool IsHitTestVisible = false;

        private bool isMouseOver = false;

        /// <summary>
        /// is mouse over
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return isMouseOver;
            }
            internal set
            {
                isMouseOver = value;
            }
        }

        /// <summary>
        /// Indicates whether the specified point is contained within this System.Drawing.Drawing2D.GraphicsPath
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsInside(int x, int y)
        {
            if (graphicsPath != null)
            {
                return graphicsPath.IsOutlineVisible(x, y, Stroke);
            }

            return false;
        }

      GraphicsPath graphicsPath;
      internal void UpdateGraphicsPath()
      {
         if(graphicsPath == null)
         {
            graphicsPath = new GraphicsPath();
         }
         else
         {
            graphicsPath.Reset();
         }

         {
            for(int i = 0; i < LocalPoints.Count; i++)
            {
               GPoint p2 = LocalPoints[i];

               if(i == 0)
               {
                  graphicsPath.AddLine(p2.X, p2.Y, p2.X, p2.Y);
               }
               else
               {
                  System.Drawing.PointF p = graphicsPath.GetLastPoint();
                  graphicsPath.AddLine(p.X, p.Y, p2.X, p2.Y);
               }
            }
         }
      }

        public virtual void OnRender(Graphics g)
        {
         if(IsVisible)
         {
            if(graphicsPath != null)
            {
               g.DrawPath(Stroke, graphicsPath);
            }
         }
        }

        public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(144, Color.MidnightBlue));

        /// <summary>
        /// specifies how the outline is painted
        /// </summary>
        [NonSerialized]
        public Pen Stroke = DefaultStroke;

        public readonly List<GPoint> LocalPoints = new List<GPoint>();

        static GMapRoute()
        {
            DefaultStroke.LineJoin = LineJoin.Round;
            DefaultStroke.Width = 5;
        }

        public GMapRoute(string name)
            : base(name)
        {

        }

        public GMapRoute(IEnumerable<PointLatLng> points, string name)
            : base(points, name)
        {

        }

        #region ISerializable Members

      // Temp store for de-serialization.
      private GPoint[] deserializedLocalPoints;

      /// <summary>
      /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
      /// </summary>
      /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
      /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
      /// <exception cref="T:System.Security.SecurityException">
      /// The caller does not have the required permission.
      /// </exception>
      public override void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info, context);

         info.AddValue("Visible", this.IsVisible);
         info.AddValue("LocalPoints", this.LocalPoints.ToArray());
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="GMapRoute"/> class.
      /// </summary>
      /// <param name="info">The info.</param>
      /// <param name="context">The context.</param>
      protected GMapRoute(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
         //this.Stroke = Extensions.GetValue<Pen>(info, "Stroke", new Pen(Color.FromArgb(144, Color.MidnightBlue)));
         this.IsVisible = Extensions.GetStruct<bool>(info, "Visible", true);
         this.deserializedLocalPoints = Extensions.GetValue<GPoint[]>(info, "LocalPoints");
      }

        #endregion

        #region IDeserializationCallback Members

      /// <summary>
      /// Runs when the entire object graph has been de-serialized.
      /// </summary>
      /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
      public override void OnDeserialization(object sender)
      {
         base.OnDeserialization(sender);

         // Accounts for the de-serialization being breadth first rather than depth first.
         LocalPoints.AddRange(deserializedLocalPoints);
         LocalPoints.Capacity = Points.Count;
      }

        #endregion

        #region IDisposable Members

        bool disposed = false;

        public virtual void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                LocalPoints.Clear();

                if (graphicsPath != null)
                {
                    graphicsPath.Dispose();
                    graphicsPath = null;
                }
                base.Clear();
            }
        }

        #endregion
    }

    //public delegate void RouteClick(GMapRoute item, MouseEventArgs e);
    public delegate void RouteEnter(GMapRoute item);
    public delegate void RouteLeave(GMapRoute item);
}
