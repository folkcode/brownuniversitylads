using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Blake.NUI.WPF.Touch;

// from Roberto Sonnino (http://virtualdreams.com.br/blog/2010/11/new-article-deep-zoom-for-wpf/)
namespace DeepZoom.Controls
{
    /// <summary>
    /// Enables users to open a multi-resolution image, which can be zoomed in on and panned across. 
    /// </summary>
    [TemplatePart(Name = "PART_ItemsControl", Type = typeof(ItemsControl))]
    public class MultiScaleImage : Control
    {
        private const int ScaleAnimationRelativeDuration = 400;
        private const double MinScaleRelativeToMinSize = 0.8;
        private double MaxScaleRelativeToMaxSize = 1.2;
        private const int ThrottleIntervalMilliseconds = 50;

        private ItemsControl _itemsControl;
        private ZoomableCanvas _zoomableCanvas;
        private MultiScaleImageSpatialItemsSource _spatialSource;

        public MultiScaleImageSpatialItemsSource SpatialSource
        {
            get { return _spatialSource; }
            set { _spatialSource = value; }
        }
        private bool inertiaEnabled = true;
        private double _originalScale;
        private int _desiredLevel;
        private readonly DispatcherTimer _levelChangeThrottle;

        static MultiScaleImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiScaleImage), new FrameworkPropertyMetadata(typeof(MultiScaleImage)));
           
        }

        public MultiScaleImage()
        {
            // new
            this.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.OnManipulationDelta_Handler);
            this.ManipulationInertiaStarting += new EventHandler<ManipulationInertiaStartingEventArgs>(this.OnManipulationInertiaStarting_Handler);
            this.PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel_Handler);


            MouseTouchDevice.RegisterEvents(this);
            _levelChangeThrottle = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ThrottleIntervalMilliseconds), IsEnabled = false };
            _levelChangeThrottle.Tick += (s, e) =>
            {
                _spatialSource.CurrentLevel = _desiredLevel;
                _levelChangeThrottle.IsEnabled = false;
            };

        }

        public void enableInertia()
        {
            if (!inertiaEnabled)
            {
                this.ManipulationInertiaStarting += this.OnManipulationInertiaStarting_Handler;
                inertiaEnabled = true;
            }
        }

        public void disableInertia()
        {
            if (inertiaEnabled)
            {
                this.ManipulationInertiaStarting -= this.OnManipulationInertiaStarting_Handler;
                inertiaEnabled = false;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            IsManipulationEnabled = true;
            _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
            if (_itemsControl == null) return;

            _itemsControl.ApplyTemplate();

            var factoryPanel = new FrameworkElementFactory(typeof(ZoomableCanvas));
            factoryPanel.AddHandler(LoadedEvent, new RoutedEventHandler(ZoomableCanvasLoaded));
            _itemsControl.ItemsPanel = new ItemsPanelTemplate(factoryPanel);

            if (_spatialSource != null)
                _itemsControl.ItemsSource = _spatialSource;
        }

        private void ZoomableCanvasLoaded(object sender, RoutedEventArgs e)
        {
            _zoomableCanvas = sender as ZoomableCanvas;
            if (_zoomableCanvas != null)
            {
                _zoomableCanvas.RealizationPriority = DispatcherPriority.Input;
                _zoomableCanvas.RealizationRate = 10;
                _zoomableCanvas.RealizationLimit = 5000;
                InitializeCanvas();
            }
        }


        #region Public methods

        /// <summary>
        /// Enables a user to zoom in on a point of the MultiScaleImage.
        /// </summary>
        /// <param name="zoomIncrementFactor">Specifies the zoom. This number is greater than 0. A value of 1 specifies that the image fit the allotted page size exactly. A number greater than 1 specifies to zoom in. If a value of 0 or less is used, failure is returned and no zoom changes are applied. </param>
        /// <param name="zoomCenterLogicalX">X coordinate for the point on the MultiScaleImage that is zoomed in on. This is a logical point (between 0 and 1). </param>
        /// <param name="zoomCenterLogicalY">Y coordinate for the point on the MultiScaleImage that is zoomed in on. This is a logical point (between 0 and 1).</param>
        public void ZoomAboutLogicalPoint(double zoomIncrementFactor, double zoomCenterLogicalX, double zoomCenterLogicalY)
        {
            var logicalPoint = new Point(zoomCenterLogicalX, zoomCenterLogicalY);
            ScaleCanvas(zoomIncrementFactor, LogicalToElementPoint(logicalPoint), true);
        }

        /// <summary>
        /// Gets a point with logical coordinates (values between 0 and 1) from a point of the MultiScaleImage. 
        /// </summary>
        /// <param name="elementPoint">The point on the MultiScaleImage to translate into a point with logical coordinates (values between 0 and 1).</param>
        /// <returns>The logical point translated from the elementPoint.</returns>
        public Point ElementToLogicalPoint(Point elementPoint)
        {
            var absoluteCanvasPoint = _zoomableCanvas.GetCanvasPoint(elementPoint);
            return new Point(absoluteCanvasPoint.X / _zoomableCanvas.Extent.Width,
                             absoluteCanvasPoint.Y / _zoomableCanvas.Extent.Height);
        }

        /// <summary>
        /// Gets a point with pixel coordinates relative to the MultiScaleImage from a logical point (values between 0 and 1).
        /// </summary>
        /// <param name="logicalPoint">The logical point to translate into pixel coordinates relative to the MultiScaleImage.</param>
        /// <returns>A point with pixel coordinates relative to the MultiScaleImage translated from logicalPoint.</returns>
        public Point LogicalToElementPoint(Point logicalPoint)
        {
            var absoluteCanvasPoint = new Point(
                logicalPoint.X * _zoomableCanvas.Extent.Width,
                logicalPoint.Y * _zoomableCanvas.Extent.Height
            );
            return _zoomableCanvas.GetVisualPoint(absoluteCanvasPoint);
        }
        public void clearCache()
        {
            _spatialSource.clearCache();
        }

        //NEW
        // The following methods were added by Brown CS (jcchin) -------------------------------------------------------------------------------------------
        public ZoomableCanvas GetZoomableCanvas
        {
            get { return _zoomableCanvas; }
        }

        /// <summary>
        /// get actual width of image
        /// </summary>
        public double GetImageActualWidth
        {
            get { return Source.ImageSize.Width; }
        }

        /// <summary>
        /// get actual height of image
        /// </summary>
        public double GetImageActualHeight
        {
            get { return Source.ImageSize.Height; }
        }

        public void EnableEventHandlers()
        {
            this.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.OnManipulationDelta_Handler);
            this.ManipulationInertiaStarting += new EventHandler<ManipulationInertiaStartingEventArgs>(this.OnManipulationInertiaStarting_Handler);
            this.PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel_Handler);
        }

        public void DisableEventHandlers()
        {
            this.ManipulationDelta -= new EventHandler<ManipulationDeltaEventArgs>(this.OnManipulationDelta_Handler);
            this.ManipulationInertiaStarting -= new EventHandler<ManipulationInertiaStartingEventArgs>(this.OnManipulationInertiaStarting_Handler);
            this.PreviewMouseWheel -= new MouseWheelEventHandler(this.OnPreviewMouseWheel_Handler);
        }

        public Uri GetImageSource()
        {
            DeepZoomImageTileSource source = (DeepZoomImageTileSource)this.Source;
            return source.UriSource;
        }

        public void SetImageSource(String ImageSourceURI)
        {
            this.Source = new DeepZoomImageTileSource(new Uri(ImageSourceURI, UriKind.Absolute));
        }

        public double ClampTargetScale(double targetScaleParameter)
        {
            var scale = _zoomableCanvas.Scale;

            // minimum size = 80% of size where the whole image is visible
            // maximum size = Max(120% of full resolution of the image, 120% of original scale)

            MaxScaleRelativeToMaxSize = 1.2;
            targetScaleParameter = targetScaleParameter.Clamp(
                MinScaleRelativeToMinSize * _originalScale,
                Math.Max(MaxScaleRelativeToMaxSize, MaxScaleRelativeToMaxSize * _originalScale));

            return targetScaleParameter;
        }

        public void LoadCurrentZoomLevelTiles()
        {
            var newLevel = Source.GetLevel(_zoomableCanvas.Scale);
            _spatialSource.CurrentLevel = newLevel;
        }

        /// <summary>
        /// used by hard-coded sample tours of Zhang Zeduan's "Along the River During the Qingming Festival" and "Garibaldi Panorama" Panel #43
        /// </summary>
        /// <param name="targetScaleParameter"></param>
        /// <param name="centeredOn"></param>
        /// <param name="durationParameter"></param>
        /// <param name="animate"></param>
        public void ScaleCanvasPoint(double targetScaleParameter, Point centeredOn, double durationParameter, bool animate)
        {
            var scale = _zoomableCanvas.Scale;

            if (scale <= 0) return;

            // minimum size = 80% of size where the whole image is visible
            // maximum size = Max(120% of full resolution of the image, 120% of original scale)

            MaxScaleRelativeToMaxSize = 1.2;
            targetScaleParameter = targetScaleParameter.Clamp(
                MinScaleRelativeToMinSize * _originalScale,
                Math.Max(MaxScaleRelativeToMaxSize, MaxScaleRelativeToMaxSize * _originalScale));

            var targetScale = targetScaleParameter;

            var newLevel = Source.GetLevel(targetScale);
            var level = _spatialSource.CurrentLevel;
            if (newLevel != level)
            {
                // If it's zooming in, throttle
                if (newLevel > level)
                    ThrottleChangeLevel(newLevel);
                else
                    _spatialSource.CurrentLevel = newLevel;
            }

            if (targetScale != scale)
            {
                Point targetOffset = new Point((centeredOn.X * targetScale) - (_zoomableCanvas.ActualWidth * 0.5), (centeredOn.Y * targetScale) - (_zoomableCanvas.ActualHeight * 0.5));

                if (animate)
                {
                    var duration = TimeSpan.FromMilliseconds(durationParameter);

                    _zoomableCanvas.BeginAnimation(ZoomableCanvas.OffsetProperty, new PointAnimation(targetOffset, duration) { }, HandoffBehavior.Compose);
                    _zoomableCanvas.BeginAnimation(ZoomableCanvas.ScaleProperty, new DoubleAnimation(targetScale, duration) { }, HandoffBehavior.Compose);
                }
                else
                {
                    _zoomableCanvas.Scale = targetScale;
                    _zoomableCanvas.Offset = targetOffset;
                }
            }
        }

        /// <summary>
        /// resets artwork - not sure if this actually works (doesn't seem to reset artwork to original zoom level sometimes)
        /// </summary>
        public void ResetArtwork()
        {
            if (_zoomableCanvas != null)
            {
                this.InitializeCanvas();
            }
        }

        #endregion

        #region Dependency Properties
        #region Source

        /// <summary>
        /// Source Dependency Property
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MultiScaleTileSource), typeof(MultiScaleImage),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnSourceChanged)));

        /// <summary>
        /// Gets or sets the Source property. This dependency property 
        /// indicates the tile source for this MultiScaleImage.
        /// </summary>
        public MultiScaleTileSource Source
        {
            get { return (MultiScaleTileSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Source property.
        /// </summary>
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiScaleImage target = (MultiScaleImage)d;
            MultiScaleTileSource oldSource = (MultiScaleTileSource)e.OldValue;
            MultiScaleTileSource newSource = target.Source;
            target.OnSourceChanged(oldSource, newSource);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Source property.
        /// </summary>
        protected virtual void OnSourceChanged(MultiScaleTileSource oldSource, MultiScaleTileSource newSource)
        {
            if (newSource == null)
            {
                _spatialSource = null;
                return;
            }

            _spatialSource = new MultiScaleImageSpatialItemsSource(newSource);

            if (_itemsControl != null)
                _itemsControl.ItemsSource = _spatialSource;

            if (_zoomableCanvas != null)
                InitializeCanvas();
        }

        #endregion

        #region AspectRatio

        /// <summary>
        /// AspectRatio Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey AspectRatioPropertyKey
            = DependencyProperty.RegisterReadOnly("AspectRatio", typeof(double), typeof(MultiScaleImage),
                new FrameworkPropertyMetadata(1.0));

        public static readonly DependencyProperty AspectRatioProperty
            = AspectRatioPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the aspect ratio of the image used as the source of the MultiScaleImage. 
        /// The aspect ratio is the width of the image divided by its height.
        /// </summary>
        public double AspectRatio
        {
            get { return (double)GetValue(AspectRatioProperty); }
        }

        /// <summary>
        /// Provides a secure method for setting the AspectRatio property.  
        /// The aspect ratio is the width of the image divided by its height.
        /// </summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetAspectRatio(double value)
        {
            SetValue(AspectRatioPropertyKey, value);
        }

        #endregion

        #endregion

        #region Overriden Input Event Handlers

        // orignal event handlers - copied below with different method signatures so that they can be added and removed
        /*protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            var oldScale = _zoomableCanvas.Scale;
            _zoomableCanvas.ApplyAnimationClock(ZoomableCanvas.ScaleProperty, null);
            _zoomableCanvas.Scale = oldScale;

            var oldOffset = _zoomableCanvas.Offset;
            _zoomableCanvas.ApplyAnimationClock(ZoomableCanvas.OffsetProperty, null);
            _zoomableCanvas.Offset = oldOffset;

            var scale = e.DeltaManipulation.Scale.X;
            ScaleCanvas(scale, e.ManipulationOrigin);

            _zoomableCanvas.Offset -= e.DeltaManipulation.Translation;
            e.Handled = true;
        }

        protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
        {
            base.OnManipulationInertiaStarting(e);
            e.TranslationBehavior = new InertiaTranslationBehavior { DesiredDeceleration = 0.0096 };
            e.ExpansionBehavior = new InertiaExpansionBehavior { DesiredDeceleration = 0.000096 };
            e.Handled = true;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            var relativeScale = Math.Pow(2, (double)e.Delta / Mouse.MouseWheelDeltaForOneLine);
            var position = e.GetPosition(_itemsControl);

            ScaleCanvas(relativeScale, position, true);

            e.Handled = true;
        }*/

        /// <summary>
        /// panning artwork via single-finger dragging gesture or mouse drag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnManipulationDelta_Handler(object sender, ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            var oldScale = _zoomableCanvas.Scale;
            _zoomableCanvas.ApplyAnimationClock(ZoomableCanvas.ScaleProperty, null);
            _zoomableCanvas.Scale = oldScale;

            var oldOffset = _zoomableCanvas.Offset;

            // jcchin - bounds (there's a better way to do this (artwork should not bounce awkwardly at boundaries (half of width and/or height))
            var minOffsetX = -(_zoomableCanvas.ActualWidth / 2);
            var maxOffsetX = (_zoomableCanvas.Scale * this.GetImageActualWidth) - (_zoomableCanvas.ActualWidth / 2);
            var minOffsetY = -(_zoomableCanvas.ActualHeight / 2);
            var maxOffsetY = (_zoomableCanvas.Scale * this.GetImageActualHeight) - (_zoomableCanvas.ActualHeight / 2);

            if (oldOffset.X > maxOffsetX)
            {
                oldOffset.X = maxOffsetX;
            }
            else if ((oldOffset.X < minOffsetX))
            {
                oldOffset.X = minOffsetX;
            }

            if (oldOffset.Y > maxOffsetY)
            {
                oldOffset.Y = maxOffsetY;
            }
            else if ((oldOffset.Y < minOffsetY))
            {
                oldOffset.Y = minOffsetY;
            }

            // jcchin - bounds

            _zoomableCanvas.ApplyAnimationClock(ZoomableCanvas.OffsetProperty, null);
            _zoomableCanvas.Offset = oldOffset;

            var scale = e.DeltaManipulation.Scale.X;
            ScaleCanvas(scale, e.ManipulationOrigin);

            _zoomableCanvas.Offset -= e.DeltaManipulation.Translation;

            // debug info for tour prep
            /*Point centeredOn = new Point();
            centeredOn.X = (_zoomableCanvas.Offset.X + (_zoomableCanvas.ActualWidth * 0.5)) / _zoomableCanvas.Scale;
            centeredOn.Y = (_zoomableCanvas.Offset.Y + (_zoomableCanvas.ActualHeight * 0.5)) / _zoomableCanvas.Scale;
            Console.WriteLine("msi.centeredOn.X = " + centeredOn.X + ", msi.centeredOn.Y = " + centeredOn.Y);
            Console.WriteLine("msi.Scale = " + _zoomableCanvas.Scale);*/

            e.Handled = true;
        }

        protected void OnManipulationInertiaStarting_Handler(object sender, ManipulationInertiaStartingEventArgs e)
        {
            base.OnManipulationInertiaStarting(e);
            e.TranslationBehavior = new InertiaTranslationBehavior { DesiredDeceleration = 0.0096 };
            e.ExpansionBehavior = new InertiaExpansionBehavior { DesiredDeceleration = 0.000096 };
            e.Handled = true;
        }

        /// <summary>
        /// zooming artwork via pinching gesture or mouse wheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnPreviewMouseWheel_Handler(object sender, MouseWheelEventArgs e)
        {
            var relativeScale = Math.Pow(2, (double)e.Delta / Mouse.MouseWheelDeltaForOneLine);
            var position = e.GetPosition(_itemsControl);

            ScaleCanvas(relativeScale, position, true);

            e.Handled = true;
        }

        #endregion

        #region Private helpers

        private void InitializeCanvas()
        {
            if (Source == null) 
                return;

            _zoomableCanvas.ApplyAnimationClock(ZoomableCanvas.ScaleProperty, null);
            _zoomableCanvas.ApplyAnimationClock(ZoomableCanvas.OffsetProperty, null);

            var level = Source.GetLevel(_zoomableCanvas.ActualWidth, _zoomableCanvas.ActualHeight);
            //var level = Source.GetLevel(1920, 1080);
            _spatialSource.CurrentLevel = level;

            var imageSize = Source.ImageSize;
            var relativeScale = Math.Min(_itemsControl.ActualWidth / imageSize.Width,
                                         _itemsControl.ActualHeight / imageSize.Height);

            _originalScale = relativeScale;

            _zoomableCanvas.Scale = _originalScale;
            _zoomableCanvas.Offset =
                new Point(imageSize.Width * 0.5 * relativeScale - _zoomableCanvas.ActualWidth * 0.5,
                          imageSize.Height * 0.5 * relativeScale - _zoomableCanvas.ActualHeight * 0.5);
            _zoomableCanvas.Clip = new RectangleGeometry(
                new Rect(0, 0,
                    imageSize.Width,
                    imageSize.Height));

            SetAspectRatio(_spatialSource.Extent.Width / _spatialSource.Extent.Height);

            _spatialSource.InvalidateSource();
        }

        private void ScaleCanvas(double relativeScale, Point center, bool animate = false)
        {
            var scale = _zoomableCanvas.Scale;

            if (scale <= 0) return;

            // minimum size = 80% of size where the whole image is visible
            // maximum size = Max(120% of full resolution of the image, 120% of original scale)

            relativeScale = relativeScale.Clamp(
                MinScaleRelativeToMinSize * _originalScale / scale,
                Math.Max(MaxScaleRelativeToMaxSize, MaxScaleRelativeToMaxSize * _originalScale) / scale);

            var targetScale = scale * relativeScale;

            var newLevel = Source.GetLevel(targetScale);
            var level = _spatialSource.CurrentLevel;
            if (newLevel != level)
            {
                // If it's zooming in, throttle
                if (newLevel > level)
                    ThrottleChangeLevel(newLevel);
                else
                    _spatialSource.CurrentLevel = newLevel;
            }

            if (targetScale != scale)
            {
                var position = (Vector)center;
                var targetOffset = (Point)((Vector)(_zoomableCanvas.Offset + position) * relativeScale - position);

                if (animate)
                {
                    if (relativeScale < 1)
                        relativeScale = 1 / relativeScale;
                    var duration = TimeSpan.FromMilliseconds(relativeScale * ScaleAnimationRelativeDuration);
                    var easing = new CubicEase();
                    _zoomableCanvas.BeginAnimation(ZoomableCanvas.ScaleProperty, new DoubleAnimation(targetScale, duration) { EasingFunction = easing }, HandoffBehavior.Compose);
                    _zoomableCanvas.BeginAnimation(ZoomableCanvas.OffsetProperty, new PointAnimation(targetOffset, duration) { EasingFunction = easing }, HandoffBehavior.Compose);
                }
                else
                {
                    _zoomableCanvas.Scale = targetScale;
                    _zoomableCanvas.Offset = targetOffset;
                    
                }
            }
        }

        private void ThrottleChangeLevel(int newLevel)
        {
            _desiredLevel = newLevel;

            if (_levelChangeThrottle.IsEnabled)
                _levelChangeThrottle.Stop();

            _levelChangeThrottle.Start();
        }

        #endregion
    }
}
