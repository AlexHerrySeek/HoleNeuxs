using System;
using System.Windows;
using System.Windows.Controls;

namespace HoleNexusLauncher.Controls
{
    public partial class SettingButton : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(SettingButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(SettingButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ShorthandProperty = DependencyProperty.Register(nameof(Shorthand), typeof(string), typeof(SettingButton), new PropertyMetadata(string.Empty));

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

        public string Shorthand
        {
            get => (string)GetValue(ShorthandProperty);
            set => SetValue(ShorthandProperty, value);
        }

        public event EventHandler<EventArgs> OnClicked;

        private void ClickButton_Click(object sender, RoutedEventArgs e)
        {
            OnClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
