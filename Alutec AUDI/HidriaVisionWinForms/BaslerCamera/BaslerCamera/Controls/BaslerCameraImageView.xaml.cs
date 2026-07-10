using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BaslerCam.Utils;

namespace BaslerCam.Controls
{
    /// <summary>
    /// Represents image view for the basler camera.
    /// </summary>
    public partial class BaslerCameraImageView : UserControl
    {
        #region Private fields
        private object LockObj;
        private int FrameCounter;
        private double Fps;
        private bool _EnableOverlay;
        private bool _InteractiveMode;
        private OverlayPosition _OverlayPos;
        private ImageOrientation _ImgOrientation;
        private ImageOrientation PrevImgOrientation;
        private IBaslerCamContinuousGrab CameraContinuousGrab;
        private WriteableBitmap ImageBoxImage;
        private Stopwatch FpsTimer;
        private System.Windows.Point Start;
        private System.Windows.Point Origin;
        #endregion

        #region Properties
        /// <summary>
        /// Enables or disables the overlay.
        /// </summary>
        public bool Overlay
        {
            get
            {
                lock (LockObj)
                {
                    return _EnableOverlay;
                }
            }
            set
            {
                lock (LockObj)
                {
                    _EnableOverlay = value;
                }
            }
        }

        /// <summary>
        /// Sets the overlay position.
        /// </summary>
        public OverlayPosition OverlayPos
        {
            get
            {
                lock (LockObj)
                {
                    return _OverlayPos;
                }
            }
            set
            {
                lock (LockObj)
                {
                    _OverlayPos = value;
                }
            }
        }

        /// <summary>
        /// Sets the image orientation.
        /// </summary>
        public ImageOrientation ImgOrientation
        {
            get
            {
                lock (LockObj)
                {
                    return _ImgOrientation;
                }
            }
            set
            {
                lock (LockObj)
                {
                    _ImgOrientation = value;
                }
            }
        }

        /// <summary>
        /// Enables or disables the interactive mode.
        /// </summary>
        public bool InteractiveMode
        {
            get
            {
                return _InteractiveMode;
            }
            set
            {
                if (value)
                {
                    // Register events
                    this.MouseWheel -= Image_MouseWheel;
                    this.MouseMove -= Image_MouseMove;
                    this.MouseLeftButtonUp -= Image_MouseLeftButtonUp;
                    this.MouseLeftButtonDown -= Image_MouseLeftButtonDown;
                    this.MouseDoubleClick -= Image_MouseLeftButtonDoubleCLlick;

                    this.MouseWheel += Image_MouseWheel;
                    this.MouseMove += Image_MouseMove;
                    this.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                    this.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                    this.MouseDoubleClick += Image_MouseLeftButtonDoubleCLlick;
                }
                else
                {
                    // Unregister events
                    this.MouseWheel -= Image_MouseWheel;
                    this.MouseMove -= Image_MouseMove;
                    this.MouseLeftButtonUp -= Image_MouseLeftButtonUp;
                    this.MouseLeftButtonDown -= Image_MouseLeftButtonDown;
                    this.MouseRightButtonDown -= Image_MouseLeftButtonDoubleCLlick;
                }

                _InteractiveMode = value;
            }
        }
        #endregion

        /// <summary>
        /// Initializes new instance of image view.
        /// </summary>
        public BaslerCameraImageView()
        {
            InitializeComponent();

            // Init vars
            ImageBoxImage = null;
            CameraContinuousGrab = null;
            FrameCounter = 0;
            Fps = 0;
            _EnableOverlay = false;
            LockObj = new object();
            FpsTimer = new Stopwatch();
            _ImgOrientation = ImageOrientation.Orientation_0;
            PrevImgOrientation = _ImgOrientation;

            // Set Imagebox render transform origin to 0, 0
            ImageBox.RenderTransformOrigin = new System.Windows.Point(0,0);

            // Create transform group for Imagebox. Add scale and translate transforms into the group
            TransformGroup TGroup = new TransformGroup();

            TGroup.Children.Add(new ScaleTransform());
            TGroup.Children.Add(new TranslateTransform());

            ImageBox.RenderTransform = TGroup;

            // Create transfomr group for ImageBoxBorder. Add rotate transform into the group
            TGroup = new TransformGroup();
            TGroup.Children.Add(new RotateTransform());

            ImageBoxBorder.LayoutTransform = TGroup;

            // Interactive mode is disabled by default
            InteractiveMode = false;
        }

        #region Private methods
        private TranslateTransform GetImageBoxTranslateTransform()
        {
            return (TranslateTransform)((TransformGroup)ImageBox.RenderTransform).Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetImageBoxScaleTransform()
        {
            return (ScaleTransform)((TransformGroup)ImageBox.RenderTransform).Children.First(tr => tr is ScaleTransform);
        }

        private RotateTransform GetImageBoxBorderRotateTransform()
        {
            return (RotateTransform)((TransformGroup)ImageBoxBorder.LayoutTransform).Children.First(tr => tr is RotateTransform);
        }

        private void ResetImageBox()
        {
            // Set image box render transform to 0.5. This is the center of the ImageBox
            ImageBox.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

            // Reset zoom
            ScaleTransform St = GetImageBoxScaleTransform();
            St.ScaleX = 1.0;
            St.ScaleY = 1.0;

            // Reset pan
            TranslateTransform Tt = GetImageBoxTranslateTransform();
            Tt.X = 0.0;
            Tt.Y = 0.0;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Attaches the camera to the image view.
        /// </summary>
        /// <param name="cameraContinuousGrab"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Attach(IBaslerCamContinuousGrab cameraContinuousGrab)
        {
            // Check if grabber is already attached
            if (CameraContinuousGrab != null)
                throw new InvalidOperationException("Grabber already attached!");

            CameraContinuousGrab = cameraContinuousGrab;

            // Register the event
            CameraContinuousGrab.ContinuousShotImageReadyEvent -= OnImageReady;
            CameraContinuousGrab.ContinuousShotImageReadyEvent += OnImageReady;
        }

        /// <summary>
        /// Detaches the camera from the image view.
        /// </summary>
        public void Detach()
        {
            // Detach the grabber and unregister the event
            CameraContinuousGrab.ContinuousShotImageReadyEvent -= OnImageReady;
            CameraContinuousGrab = null;
        }
        #endregion

        #region Events
        private void OnImageReady(object sender, ImageReadyEventArgs e)
        {
            bool ImgOrientationChanged = false;
            bool EnableOverlay;
            OverlayPosition OverlayPos;
            ImageOrientation ImgOrientation;
            Bitmap GrabbedImage = e.GrabbedImage;

            lock (LockObj)
            {
                EnableOverlay = this._EnableOverlay;
                OverlayPos = this._OverlayPos;
                ImgOrientation = this._ImgOrientation;
            }

            // Calculate FPS
            FrameCounter++;
            if (FrameCounter == 15)
            {
                Fps = FrameCounter / (FpsTimer.ElapsedMilliseconds / 1000.0);
                FpsTimer.Restart();
                FrameCounter = 0;
            }

            // Draw an overlay if enabled
            if (EnableOverlay)
                ImageUtils.AddOverlay(OverlayPos, Fps, ImgOrientation, ref GrabbedImage);

            // Check if image orientation has been changed
            if (ImgOrientation != PrevImgOrientation)
            {
                ImgOrientationChanged = true;
                PrevImgOrientation = ImgOrientation;
            }

            // Update image inside the Imagebox
            this.Dispatcher.BeginInvoke(new Action(() => {
                using (GrabbedImage)
                {
                    // Check if Imagebox image has correct dimentions. If not create new writeable bitmap object
                    if (ImageBoxImage == null || !ImageUtils.IsWriteableBitmapCompatible(ImageBoxImage, GrabbedImage))
                    {
                        ImageBoxImage = ImageUtils.CreateWriteableBitmap(GrabbedImage.Width, GrabbedImage.Height);
                    }

                    // Update Imagebox image
                    ImageUtils.UpdateWritableBitmap(ref ImageBoxImage, GrabbedImage);

                    // Apply image orientation changes if necessary
                    if (ImgOrientationChanged)
                    {
                        // Reset zoom and pan
                        ResetImageBox();

                        // Set the orientation of the ImageBoxBorder
                        RotateTransform RotTransf = GetImageBoxBorderRotateTransform();
                        RotTransf.Angle = (double)ImgOrientation;
                    }

                    // Update Imagebox
                    ImageBox.Source = ImageBoxImage;
                    ImageBox.InvalidateVisual();
                }
            }));
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScaleTransform St = GetImageBoxScaleTransform();
            TranslateTransform Tt = GetImageBoxTranslateTransform();
            RotateTransform Rt = GetImageBoxBorderRotateTransform();
            System.Windows.Point MouseLocation = e.GetPosition(ImageBox);
            double Zoom = e.Delta > 0 ? .1 : -.1;
            double AbosuluteX;
            double AbosuluteY;

            // Set Imagebox render transform origin to 0, 0
            ImageBox.RenderTransformOrigin = new System.Windows.Point(0,0);

            // Limit the zoom
            if (Zoom < 0 && (St.ScaleX < .3 || St.ScaleY < .3))
                return;

            // Calculate Scale and translation
            AbosuluteX = MouseLocation.X * St.ScaleX + Tt.X;
            AbosuluteY = MouseLocation.Y * St.ScaleY + Tt.Y;

            St.ScaleX += St.ScaleX * Zoom;
            St.ScaleY += St.ScaleY * Zoom;

            Tt.X = AbosuluteX - MouseLocation.X * St.ScaleX;
            Tt.Y = AbosuluteY - MouseLocation.Y * St.ScaleY;
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (ImageBox.IsMouseCaptured)
            {
                TranslateTransform Tt = GetImageBoxTranslateTransform();
                RotateTransform Rt = GetImageBoxBorderRotateTransform();
                Vector V = Start - e.GetPosition(ImageBoxBorder);

                // Calculate translation
                Tt.X = Origin.X - V.X;
                Tt.Y = Origin.Y - V.Y;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Release the mouse after dragging
            ImageBox.ReleaseMouseCapture();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TranslateTransform Tt = GetImageBoxTranslateTransform();

            Start = e.GetPosition(ImageBoxBorder);
            Origin = new System.Windows.Point(Tt.X, Tt.Y);

            // Capture the mouse for dragging
            ImageBox.CaptureMouse();
        }

        private void Image_MouseLeftButtonDoubleCLlick(object sender, MouseButtonEventArgs e)
        {
            // Check if left button has been clicked and reset zoom and pan
            if (e.ChangedButton == MouseButton.Left)
                ResetImageBox();
        }
        #endregion
    }
}
