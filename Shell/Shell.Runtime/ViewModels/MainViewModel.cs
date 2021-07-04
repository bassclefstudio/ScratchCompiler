using BassClefStudio.AppModel.Commands;
using BassClefStudio.AppModel.Navigation;
using BassClefStudio.AppModel.Storage;
using BassClefStudio.AppModel.Threading;
using BassClefStudio.Graphics.Core;
using BassClefStudio.Graphics.Input;
using BassClefStudio.NET.Core;
using BassClefStudio.NET.Core.Streams;
using BassClefStudio.Shell.Runtime.Processor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Runtime.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : IViewModel
    {
        public static readonly CommandInfo OpenCommand = new CommandInfo()
        {
            Id = "Main/Open",
            FriendlyName = "Open code folder",
            Description = "Choose a folder to find microcode and bootloader output files."
        };

        public static readonly CommandInfo<string> SelectCommand = new CommandInfo<string>()
        {
            Id = "Main/Select",
            FriendlyName = "Select code file",
            Description = "Choose a file to run in the processor.",
            InputDescription = "The name of the file, without an extension."
        };

        public static readonly CommandInfo StartCommand = new CommandInfo()
        {
            Id = "Main/Start",
            FriendlyName = "Start",
            Description = "Start processor execution."
        };

        /// <inheritdoc/>
        public IList<ICommand> Commands { get; }

        public ObservableCollection<string> ConsoleList { get; }

        public ObservableCollection<string> FileList { get; }

        /// <summary>
        /// The injected <see cref="RuntimeConfiguration"/>.
        /// </summary>
        public RuntimeConfiguration Configuration { get; }

        /// <summary>
        /// The injected <see cref="IStorageService"/>.
        /// </summary>
        public IStorageService StorageService { get; set; }

        /// <summary>
        /// The injected collection of <see cref="IDispatcher"/>s.
        /// </summary>
        public IEnumerable<IDispatcher> Dispatchers { get; set; }

        /// <summary>
        /// The injected <see cref="ISystem"/>.
        /// </summary>
        public ISystem System { get; set; }

        /// <summary>
        /// Creates a new <see cref="MainViewModel"/>.
        /// </summary>
        public MainViewModel(RuntimeConfiguration configuration)
        {
            Configuration = configuration;
            Configuration.ConsoleWriter = new SourceStream<string>();
            ConsoleList = new ObservableCollection<string>();
            Configuration.ConsoleWriter.BindResult(t => Dispatchers.RunOnUIThreadAsync(() => ConsoleList.Add(t)));

            FileList = new ObservableCollection<string>();

            var open = new StreamCommand(OpenCommand);
            open.BindResult(o => Dispatchers.RunOnUIThreadAsync(() => OpenFolder()));

            var select = new StreamCommand<string>(SelectCommand);
            select.BindResult(o => Dispatchers.RunOnUIThreadAsync(() => LoadFile(o)));

            var start = new StreamCommand(StartCommand);
            start.BindResult(b => Task.Run(StartProcess));

            Commands = new List<ICommand>()
            {
                open,
                select,
                start
            };
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(object parameter = null)
        { }

        public void InitializeGraphics(IGraphicsView view, IInputWatcher input)
        {
            Configuration.GraphicsView = view;
            Configuration.InputWatcher = input;
        }

        private IStorageFolder folder;
        internal async Task<bool> OpenFolder()
        {
            folder = await StorageService.RequestFolderAsync(new StorageDialogSettings());
            if (folder != null)
            {
                var items = (await folder.GetItemsAsync()).ToArray();
                FileList.Clear();
                FileList.AddRange(items.OfType<IStorageFile>()
                    .Where(i => i.FileType == ".cco")
                    .Select(f => f.GetNameWithoutExtension())
                    .ToArray());
                if (await folder.ContainsItemAsync("Microcode.mco"))
                {
                    var mFile = await folder.GetFileAsync("Microcode.mco");
                    Configuration.MicrocodeFile = await mFile.ReadTextAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal async Task<bool> LoadFile(string name)
        {
            if (folder != null)
            {
                var codeFile = await folder.GetFileAsync($"{name}.cco");
                var debugFile = await folder.GetFileAsync($"{name}.ccd");
                Configuration.BootFile = await codeFile.ReadTextAsync();
                Configuration.DebugInfo = JObject.Parse(await debugFile.ReadTextAsync());
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void StartProcess()
        {
            System.Initialize();
        }
    }
}
