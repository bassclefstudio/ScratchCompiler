using BassClefStudio.Graphics.Transforms;
using BassClefStudio.Graphics.Turtle;
using BassClefStudio.NET.Core.Primitives;
using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Devices
{
    public class Display : IDeviceDriver
    {
        /// <inheritdoc/>
        public string Name { get; } = "Display";

        /// <inheritdoc/>
        public MemoryChunk MainChunk { get; }

        /// <inheritdoc/>
        public IEnumerable<MemoryChunk> Chunks { get; }

        /// <inheritdoc/>
        public RuntimeConfiguration Configuration { get; set; }

        public Dictionary<int, DisplayCommand> Commands { get; private set; }

        public Color[] Pens { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Display"/>.
        /// </summary>
        public Display()
        {
            MainChunk = new MemoryChunk(8004, 6);
            Chunks = new MemoryChunk[] { MainChunk };
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            Commands = new Dictionary<int, DisplayCommand>();
            Pens = new Color[]
            {
                new Color(255,255,255),
                new Color(255,0,0),
                new Color(0,255,0),
                new Color(0,0,255),
                new Color(255,255,0),
                new Color(0,255,255),
                new Color(255,0,255),
                new Color(0,0,0),
            };

            Configuration.GraphicsView.UpdateRequested += GraphicsRequested;
        }

        int dispCmd = 0;
        int id = 0;
        DisplayCommand currentCommand = new DisplayCommand();
        /// <inheritdoc/>
        public string HandleRequest(MemoryRequest request)
        {
            uint address = request.Address - MainChunk.Address;
            if (request.Action == MemoryAction.Poke)
            {
                if (dispCmd == 3)
                {
                    Commands.Clear();
                }
                else if (dispCmd == 2)
                {
                    Commands.Remove(id);
                }
                else if (dispCmd == 1 || dispCmd == 0)
                {
                    if(Commands.ContainsKey(id))
                    {
                        Commands[id] = currentCommand;
                    }
                    else
                    {
                        Commands.Add(id, currentCommand);
                    }

                    if(dispCmd == 1)
                    {
                        Configuration.GraphicsView.RequestUpdate();
                    }
                }
                else
                {
                    throw new ArgumentException($"The given memory request attempted display action {dispCmd} which was not found.", "request");
                }
                return null;
            }
            else if (request.Action == MemoryAction.Set)
            {
                if (address == 0)
                {
                    dispCmd = int.Parse(request.Value);
                }
                else if(address == 1)
                {
                    currentCommand.X = float.Parse(request.Value);
                }
                else if (address == 2)
                {
                    currentCommand.Y = float.Parse(request.Value);
                }
                else if (address == 3)
                {
                    currentCommand.Pen = int.Parse(request.Value);
                }
                else if (address == 4)
                {
                    currentCommand.Size = float.Parse(request.Value);
                }
                else if (address == 5)
                {
                    id = int.Parse(request.Value);
                }
                else
                {
                    throw new MemoryAccessException("This device could not find the requested memory.", request);
                }
                return null;
            }
            else if (request.Action == MemoryAction.Get)
            {
                if (address == 0)
                {
                    return dispCmd.ToString();
                }
                else if (address == 1)
                {
                    return currentCommand.X.ToString();
                }
                else if (address == 2)
                {
                    return currentCommand.Y.ToString();
                }
                else if (address == 3)
                {
                    return currentCommand.Pen.ToString();
                }
                else if (address == 4)
                {
                    return currentCommand.Size.ToString();
                }
                else if (address == 5)
                {
                    return id.ToString();
                }
                else
                {
                    throw new MemoryAccessException("This device could not find the requested memory.", request);
                }
            }
            else
            {
                throw new ArgumentException($"The given memory request attempted action {request.Action} which was not supported.", "request");
            }
        }

        private void GraphicsRequested(object sender, BassClefStudio.Graphics.Core.UpdateRequestEventArgs e)
        {
            if (e.GraphicsProvider is ITurtleGraphicsProvider turtle)
            {
                turtle.Clear(new Color(0, 0, 0));
                turtle.Camera = new ViewCamera(
                    e.ViewSize.GetValueOrDefault(new Vector2(480, 360)),
                    new Vector2(480, 360),
                    ZoomType.FitAll,
                    true);

                Vector2 lastPos = new Vector2(0, 0);
                foreach (var cmd in Commands.ToArray())
                {
                    Vector2 newPos = new Vector2(cmd.Value.X, cmd.Value.Y);
                    if (cmd.Value.Pen != 0)
                    {
                        turtle.DrawLine(lastPos, newPos, Pens[cmd.Value.Pen - 1], cmd.Value.Size, PenType.Round);
                    }
                    lastPos = newPos;
                }
            }
        }
    }

    public struct DisplayCommand
    {
        public float X { get; set; }
        public float Y { get; set; }

        public int Pen { get; set; }
        public float Size { get; set; }
    }
}
