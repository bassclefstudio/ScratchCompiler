using BassClefStudio.AppModel.Commands;
using BassClefStudio.AppModel.Navigation;
using BassClefStudio.Graphics.Core;
using BassClefStudio.NET.Core.Streams;
using BassClefStudio.Shell.Runtime.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BassClefStudio.Shell.Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IView<MainViewModel>
    {
        /// <inheritdoc/>
        public MainViewModel ViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        // <inheritdoc/>
        public void Initialize()
        {
            var graphics = new Win2DGraphicsView(graphicsView);
            ViewModel.InitializeGraphics(graphics, graphics);
        }

        private void openClicked(object sender, RoutedEventArgs e)
        {
            CommandRouter.Execute(MainViewModel.OpenCommand);
        }

        private void startClicked(object sender, RoutedEventArgs e)
        {
            CommandRouter.Execute(MainViewModel.StartCommand);
        }

        private void selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommandRouter.Execute(MainViewModel.SelectCommand, e.AddedItems.FirstOrDefault());
        }
    }
}
