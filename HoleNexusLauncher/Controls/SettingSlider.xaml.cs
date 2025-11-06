using HoleNexus.Classes;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HoleNexusLauncher.Controls
{
    public partial class SettingSlider : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(SettingSlider),
                new PropertyMetadata(0.0, OnValueChangedInternal));

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register(nameof(MinimumValue), typeof(int), typeof(SettingSlider),
                new PropertyMetadata(0));

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register(nameof(MaximumValue), typeof(int), typeof(SettingSlider),
                new PropertyMetadata(100));

        public static readonly DependencyProperty RoundingFactorProperty =
            DependencyProperty.Register(nameof(RoundingFactor), typeof(double), typeof(SettingSlider),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SettingSlider),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(SettingSlider),
                new PropertyMetadata(string.Empty));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int MinimumValue
        {
            get => (int)GetValue(MinimumValueProperty);
            set => SetValue(MinimumValueProperty, value);
        }

        public int MaximumValue
        {
            get => (int)GetValue(MaximumValueProperty);
            set => SetValue(MaximumValueProperty, value);
        }

        public double RoundingFactor
        {
            get => (double)GetValue(RoundingFactorProperty);
            set => SetValue(RoundingFactorProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public event EventHandler<EventArgs> ValueChanged;

        public SettingSlider()
        {
            InitializeComponent();
            Loaded += SettingSliderControl_Loaded;
        }

        private static void OnValueChangedInternal(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = (SettingSlider)d;

            slider.ValueChanged?.Invoke(slider, EventArgs.Empty);

            if (slider.IndicatorLabel != null)
            {
                slider.IndicatorLabel.Content = Math.Round(slider.Value).ToString();
            }

            if (slider.IndicatorGrid?.ActualWidth > 0 && slider.IndicatorHighlight != null)
            {
                double percent = (slider.Value - slider.MinimumValue) / (slider.MaximumValue - slider.MinimumValue);
                double targetWidth = percent * slider.IndicatorGrid.ActualWidth;

                Animation.Animate(new AnimationPropertyBase(slider.IndicatorHighlight)
                {
                    Property = FrameworkElement.WidthProperty,
                    To = Math.Round(targetWidth)
                });
            }
        }

        private void SettingSliderControl_Loaded(object sender, RoutedEventArgs e)
        {
            OnValueChangedInternal(this, new DependencyPropertyChangedEventArgs(ValueProperty, null, Value));
        }

        private async void IndicatorBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(IndicatorGrid);

            while (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                double x = Mouse.GetPosition(IndicatorGrid).X;
                x = Math.Max(0, Math.Min(x, IndicatorGrid.ActualWidth));

                double ratio = x / IndicatorGrid.ActualWidth;
                double raw = MinimumValue + (MaximumValue - MinimumValue) * ratio;

                Value = Math.Round(raw / RoundingFactor) * RoundingFactor;

                await Task.Delay(16); // ~60fps
            }

            Mouse.Capture(null);
        }
    }
}
