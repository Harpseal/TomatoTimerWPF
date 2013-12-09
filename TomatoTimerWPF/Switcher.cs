using System.Windows.Controls;

namespace TomatoTimerWPF
{
    public static class Switcher
    {
        public static MainWindow baseWindow;

        public static void Switch(UserControl newPage)
        {
            baseWindow.Navigate(newPage);
            baseWindow.UpdateUI();
        }

        public static void Switch(UserControl newPage, object state)
        {
            baseWindow.Navigate(newPage, state);
            baseWindow.UpdateUI();
        }

        public static void Close()
        {
            baseWindow.Close();
        }

        public static MainWindow GetBaseWindow() { return baseWindow; }
    }
}
