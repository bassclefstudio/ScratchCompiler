using BassClefStudio.AppModel.Lifecycle;
using BassClefStudio.Shell.Runtime;

namespace BassClefStudio.Shell.Client
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : UwpApplication
    {
        public App() : base(new RuntimeApp(), typeof(App).Assembly)
        { }
    }
}
