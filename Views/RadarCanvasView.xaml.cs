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
            Loaded += RadarCanvasView_Loaded;
            DataContextChanged += RadarCanvasView_DataContextChanged;
        }

        private void RadarCanvasView_Loaded(object sender, RoutedEventArgs e)
        {
            ReDrawAll();
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

            ReDrawAll();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RadarViewModel.MaxRange))
            {
                ReDrawAll();
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
            ReDrawAll();
        }

        private void ReDrawAll()
        {
            DrawBackgrounGrid();
            RedrawTargets();
        }

        /// <summary>
        /// 레이더 배경 그리드 (동심원 링 및 30도 방사선) 렌더링
        /// </summary>
        private void DrawBackgrounGrid()
        {
            RadarCanvas.Children.Clear();
            RadarCanvas.Children.Add(RadarSweep);
            RadarCanvas.Children.Add(SweepLine);
            RadarCanvas.Children.Add(TargetCanvas);

            double width = RadarCanvas.ActualWidth;
            double height = RadarCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            double centerX = width / 2;
            double centerY = height / 2;
            double radius = Math.Min(width, height) / 2 - 20;
            if (radius <= 0) return;

            // 스위프 피자 조각 Geometry 다이내믹 설정
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
            RadarSweep.Data = sectorGeo;
            SweepRotateTransform.CenterX = centerX;
            SweepRotateTransform.CenterY = centerY;

            // 스위프 탐지 회전선 설정
            SweepLine.X1 = centerX;
            SweepLine.Y1 = centerY;
            SweepLine.X2 = centerX;
            SweepLine.Y2 = centerY - radius;
            SweepLineRotateTransform.CenterX = centerX;
            SweepLineRotateTransform.CenterY = centerY;

            Brush gridBrush = (Brush)FindResource("GridLineBrush");

            // 1. 동심원 거리 링 (5단계 분할)
            int ringCount = 5;
            for (int i = 1; i <= ringCount; i++)
            {
                double r = (radius / ringCount) * i;
                Ellipse circle = new Ellipse
                {
                    Width = r * 2,
                    Height = r * 2,
                    Stroke = gridBrush,
                    StrokeThickness = i == ringCount ? 1.5 : 0.8,
                    StrokeDashArray = i == ringCount ? null : new DoubleCollection { 4, 4 },
                    Opacity = 0.6
                };
                Canvas.SetLeft(circle, centerX - r);
                Canvas.SetTop(circle, centerY - r);
                RadarCanvas.Children.Add(circle);

                // 거리 라벨 표시
                double distanceValue = (_viewModel?.MaxRange ?? 5.0) / ringCount * i;
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

            // 2. 방위각 방사선 (30도 간격)
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

                // 각도 라벨 표시
                if (deg != 0)
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
        }

        /// <summary>
        /// 극좌표계 (Distance, Angle) -> 2D Canvas (X, Y) 직교좌표 변환 핵심 로직
        /// </summary>
        private Point PolarToCartesian(double distance, double angleDegree, double maxRange)
        {
            double width = RadarCanvas.ActualWidth;
            double height = RadarCanvas.ActualHeight;

            double centerX = width / 2;
            double centerY = height / 2;
            double maxRadius = Math.Min(width, height) / 2 - 20;

            if (maxRange <= 0) maxRange = 5.0;

            // 극좌표 -> 직교좌표 삼각함수 변환 공식
            double normalizedDistance = Math.Min(distance / maxRange, 1.0);
            double r = normalizedDistance * maxRadius;
            double rad = angleDegree * Math.PI / 180.0;

            double x = centerX + r * Math.Sin(rad);
            double y = centerY - r * Math.Cos(rad);

            return new Point(x, y);
        }

        /// <summary>
        /// ViewModel의 Targets 컬렉션을 극좌표 변환하여 Canvas에 점(Blip)으로 그리기
        /// </summary>
        private void RedrawTargets()
        {
            TargetCanvas.Children.Clear();
            if (_viewModel == null) return;

            double maxRange = _viewModel.MaxRange <= 0 ? 5.0 : _viewModel.MaxRange;

            foreach (var target in _viewModel.Targets)
            {
                // 극좌표 변환 로직 호출
                Point pos = PolarToCartesian(target.Distance, target.Angle, maxRange);

                Brush targetColor = target.DangerLevel switch
                {
                    "Danger" => (Brush)FindResource("DangerBrush"),
                    "Warning" => (Brush)FindResource("WarningBrush"),
                    _ => (Brush)FindResource("NormalBrush")
                };

                // 1. 타겟 은은한 후광 (Glow Ring)
                Ellipse glow = new Ellipse
                {
                    Width = 22,
                    Height = 22,
                    Fill = targetColor,
                    Opacity = target.IsSelected ? 0.4 : 0.15
                };
                Canvas.SetLeft(glow, pos.X - 11);
                Canvas.SetTop(glow, pos.Y - 11);

                // 2. 핵심 타겟 점 (Dot)
                Ellipse dot = new Ellipse
                {
                    Width = target.IsSelected ? 12 : 8,
                    Height = target.IsSelected ? 12 : 8,
                    Fill = targetColor,
                    Stroke = target.IsSelected ? Brushes.White : null,
                    StrokeThickness = target.IsSelected ? 2 : 0,
                    Cursor = Cursors.Hand,
                    ToolTip = $"장비: {target.DeviceId}\n거리: {target.Distance:F1}m\n각도: {target.Angle:F1}°"
                };

                var closureTarget = target;
                dot.MouseDown += (s, e) =>
                {
                    _viewModel.SelectedTarget = closureTarget;
                    e.Handled = true;
                };

                Canvas.SetLeft(dot, pos.X - (dot.Width / 2));
                Canvas.SetTop(dot, pos.Y - (dot.Height / 2));

                // 3. 타겟 라벨 (DeviceId)
                TextBlock label = new TextBlock
                {
                    Text = target.DeviceId,
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = target.IsSelected ? FontWeights.Bold : FontWeights.Normal
                };
                Canvas.SetLeft(label, pos.X + 8);
                Canvas.SetTop(label, pos.Y - 6);

                TargetCanvas.Children.Add(glow);
                TargetCanvas.Children.Add(dot);
                TargetCanvas.Children.Add(label);
            }
        }
    }
}
