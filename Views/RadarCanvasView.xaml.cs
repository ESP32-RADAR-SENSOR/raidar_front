using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RAIDAR_FRONT.ViewModels;

namespace RAIDAR_FRONT.Views
{
    public partial class RadarCanvasView : UserControl
    {
        private RadarViewModel? _viewModel;

        public RadarCanvasView()
        {
            InitializeComponent();
            DataContextChanged += RadarCanvasView_DataContextChanged;
        }

        private void RadarCanvasView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Targets.CollectionChanged -= Targets_CollectionChanged;
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _viewModel = DataContext as RadarViewModel;

            if (_viewModel != null)
            {
                _viewModel.Targets.CollectionChanged += Targets_CollectionChanged;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                SubscribeTargetEvents();
            }

            RedrawAll();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RadarViewModel.MaxRange))
            {
                RedrawAll();
            }
        }

        private void Targets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SubscribeTargetEvents();
            RedrawTargets();
        }

        private void SubscribeTargetEvents()
        {
            if (_viewModel == null) return;
            foreach (var target in _viewModel.Targets)
            {
                target.PropertyChanged -= Target_PropertyChanged;
                target.PropertyChanged += Target_PropertyChanged;
            }
        }

        private void Target_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(RedrawTargets);
        }

        private void RadarCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawAll();
        }

        private void RedrawAll()
        {
            DrawBackgroundGrid();
            RedrawTargets();
        }

        private void DrawBackgroundGrid()
        {
            RadarCanvas.Children.Clear();
            RadarCanvas.Children.Add(SweepSector);
            RadarCanvas.Children.Add(SweepLine);
            RadarCanvas.Children.Add(TargetsOverlayCanvas);

            double width = RadarCanvas.ActualWidth;
            double height = RadarCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            double centerX = width / 2;
            double centerY = height / 2;
            double radius = Math.Min(width, height) / 2 - 20;

            if (radius <= 0) return;

            // 스위프 피자 모양 패스 설정
            PathGeometry sectorGeo = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = new Point(centerX, centerY) };
            double sectorAngleRad = 30 * Math.PI / 180;
            figure.Segments.Add(new LineSegment(new Point(centerX, centerY - radius), true));
            figure.Segments.Add(new ArcSegment(
                new Point(centerX + radius * Math.Sin(sectorAngleRad), centerY - radius * Math.Cos(sectorAngleRad)),
                new Size(radius, radius),
                0, false, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(new Point(centerX, centerY), true));
            sectorGeo.Figures.Add(figure);
            SweepSector.Data = sectorGeo;
            SweepRotateTransform.CenterX = centerX;
            SweepRotateTransform.CenterY = centerY;

            // 스위프 선
            SweepLine.X1 = centerX;
            SweepLine.Y1 = centerY;
            SweepLine.X2 = centerX;
            SweepLine.Y2 = centerY - radius;
            LineRotateTransform.CenterX = centerX;
            LineRotateTransform.CenterY = centerY;

            Brush gridBrush = (Brush)FindResource("GridLineBrush");

            // 동심원 동시 그리기 (5단계 분할)
            int rings = 5;
            for (int i = 1; i <= rings; i++)
            {
                double r = (radius / rings) * i;
                Ellipse circle = new Ellipse
                {
                    Width = r * 2,
                    Height = r * 2,
                    Stroke = gridBrush,
                    StrokeThickness = i == rings ? 1.5 : 0.8,
                    StrokeDashArray = i == rings ? null : new DoubleCollection { 4, 4 },
                    Opacity = 0.6
                };
                Canvas.SetLeft(circle, centerX - r);
                Canvas.SetTop(circle, centerY - r);
                RadarCanvas.Children.Add(circle);

                // 거리 라벨
                double distanceValue = (_viewModel?.MaxRange ?? 5.0) / rings * i;
                TextBlock label = new TextBlock
                {
                    Text = $"{distanceValue:F1}m",
                    Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)),
                    FontSize = 10
                };
                Canvas.SetLeft(label, centerX + 4);
                Canvas.SetTop(label, centerY - r - 6);
                RadarCanvas.Children.Add(label);
            }

            // 방위각 방사형 선 그리기 (30도 간격)
            for (int deg = 0; deg < 360; deg += 30)
            {
                double rad = deg * Math.PI / 180.0;
                double x2 = centerX + radius * Math.Sin(rad);
                double y2 = centerY - radius * Math.Cos(rad);

                Line line = new Line
                {
                    X1 = centerX,
                    Y1 = centerY,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = gridBrush,
                    StrokeThickness = 0.7,
                    Opacity = 0.5
                };
                RadarCanvas.Children.Add(line);

                // 각도 라벨
                if (deg % 90 != 0 && deg != 0)
                {
                    TextBlock degLabel = new TextBlock
                    {
                        Text = $"{deg}°",
                        Foreground = new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69)),
                        FontSize = 9
                    };
                    double lx = centerX + (radius + 12) * Math.Sin(rad) - 6;
                    double ly = centerY - (radius + 12) * Math.Cos(rad) - 6;
                    Canvas.SetLeft(degLabel, lx);
                    Canvas.SetTop(degLabel, ly);
                    RadarCanvas.Children.Add(degLabel);
                }
            }

            // 원점 중심점
            Ellipse centerDot = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(Color.FromRgb(0x00, 0xF0, 0xFF))
            };
            Canvas.SetLeft(centerDot, centerX - 3);
            Canvas.SetTop(centerDot, centerY - 3);
            RadarCanvas.Children.Add(centerDot);
        }

        private void RedrawTargets()
        {
            TargetsOverlayCanvas.Children.Clear();
            if (_viewModel == null) return;

            double width = RadarCanvas.ActualWidth;
            double height = RadarCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            double centerX = width / 2;
            double centerY = height / 2;
            double maxRadius = Math.Min(width, height) / 2 - 20;
            double maxRange = _viewModel.MaxRange <= 0 ? 5.0 : _viewModel.MaxRange;

            foreach (var target in _viewModel.Targets)
            {
                double normalizedDistance = Math.Min(target.Distance / maxRange, 1.0);
                double r = normalizedDistance * maxRadius;
                double rad = target.Angle * Math.PI / 180.0;

                double x = centerX + r * Math.Sin(rad);
                double y = centerY - r * Math.Cos(rad);

                Brush targetColor = target.DangerLevel switch
                {
                    "Danger" => (Brush)FindResource("DangerBrush"),
                    "Warning" => (Brush)FindResource("WarningBrush"),
                    _ => (Brush)FindResource("NormalBrush")
                };

                // 외곽 은은한 후광 (Glow Ring)
                Ellipse glow = new Ellipse
                {
                    Width = 24,
                    Height = 24,
                    Fill = targetColor,
                    Opacity = target.IsSelected ? 0.35 : 0.15
                };
                Canvas.SetLeft(glow, x - 12);
                Canvas.SetTop(glow, y - 12);

                // 핵심 블립 (Blip Dot)
                Ellipse dot = new Ellipse
                {
                    Width = target.IsSelected ? 12 : 8,
                    Height = target.IsSelected ? 12 : 8,
                    Fill = targetColor,
                    Stroke = target.IsSelected ? Brushes.White : null,
                    StrokeThickness = target.IsSelected ? 2 : 0,
                    Cursor = Cursors.Hand,
                    ToolTip = $"{target.DeviceId}\n거리: {target.Distance:F2}m\n각도: {target.Angle:F1}°\n온도: {target.Temperature:F1}°C"
                };

                var closureTarget = target;
                dot.MouseDown += (s, e) =>
                {
                    _viewModel.SelectedTarget = closureTarget;
                    e.Handled = true;
                };

                Canvas.SetLeft(dot, x - (dot.Width / 2));
                Canvas.SetTop(dot, y - (dot.Height / 2));

                // 타겟 ID 라벨
                TextBlock label = new TextBlock
                {
                    Text = target.DeviceId,
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = target.IsSelected ? FontWeights.Bold : FontWeights.Normal
                };
                Canvas.SetLeft(label, x + 8);
                Canvas.SetTop(label, y - 6);

                TargetsOverlayCanvas.Children.Add(glow);
                TargetsOverlayCanvas.Children.Add(dot);
                TargetsOverlayCanvas.Children.Add(label);
            }
        }
    }
}
