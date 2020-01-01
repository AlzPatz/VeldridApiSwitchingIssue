using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Veldrid.StartupUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiSwitchingIssue
{
    class Demo
    {
        private Sdl2Window _window;
        private GraphicsDevice _device;
        private DisposeCollectorResourceFactory _factory;
        private CommandList _cl;

        private List<GraphicsBackend> _apis;
        private int _apiIndex;

        private int _frameCount = 0;
        private const int _periodNumberFrames = 120;

        public void Run()
        {
            ExtractValidAPIs();

            Init();

            Loop();

            ReleaseResources();
        }

        private void ExtractValidAPIs()
        {
            //Comment those you want to include in the rotation (if valid for system)
            var apisExcluded = new List<GraphicsBackend>
            {
                GraphicsBackend.OpenGLES,
                //GraphicsBackend.OpenGL,
                GraphicsBackend.Direct3D11,
                //GraphicsBackend.Vulkan,
                GraphicsBackend.Metal
            };

            _apis = ((GraphicsBackend[])Enum.GetValues(typeof(GraphicsBackend))).Where(x => !apisExcluded.Contains(x) && GraphicsDevice.IsBackendSupported(x)).ToList();

            _apiIndex = 0;

            Console.WriteLine("APIs selected to iterate between:");
            _apis.ForEach(x => Console.WriteLine(x.ToString()));
        }

        private void Init()
        {
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Potential Issue Switching APIs"
            };

            _window = VeldridStartup.CreateWindow(ref windowCI);

            ReCreateGraphicsDevice();
        }

        private void ReCreateGraphicsDevice()
        {
            _factory?.DisposeCollector.DisposeAll();
            _device?.Dispose();

            var options = new GraphicsDeviceOptions(
              debug: true,
              swapchainDepthFormat: PixelFormat.R16_UNorm,
              syncToVerticalBlank: true,
              resourceBindingModel: ResourceBindingModel.Improved);

            _device = VeldridStartup.CreateGraphicsDevice(_window, options, _apis[_apiIndex]);
            _factory = new DisposeCollectorResourceFactory(_device.ResourceFactory);
            _cl = _factory.CreateCommandList();

            //ERROR on change from OpenGL to Vulkan (in WINDOWs during my testing)     
            //With debug : false -> it is a memory access violation
            //With debug : true -> it is Veldrid.VeldridException. HResult = 0x80131500. Message = A Vulkan validation error was encountered: [ErrorEXT](SwapchainKHREXT) vkCreateSwapchainKHR: internal drawable creation failed
        }

        private void Loop()
        {
            while (_window.Exists)
            {
                _window.PumpEvents();
                Update();
                Render();
            }

            ReleaseResources();
        }

        private void Update()
        {
            _frameCount++;

            if (_frameCount == _periodNumberFrames)
            {
                _frameCount = 0;

                var currentAPI = _apis[_apiIndex].ToString();

                _apiIndex++;
                if (_apiIndex == _apis.Count)
                {
                    _apiIndex = 0;
                }

                var nextAPI = _apis[_apiIndex].ToString();

                Console.WriteLine(string.Concat("Trying to switch from ", currentAPI, " to ", nextAPI));
                ReCreateGraphicsDevice();
                Console.WriteLine("Success!");
            }
        }

        private void Render()
        {
            _cl.Begin();

            _cl.SetFramebuffer(_device.SwapchainFramebuffer);

            _cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

            _cl.End();

            _device.SubmitCommands(_cl);

            _device.SwapBuffers();

            _device.WaitForIdle();
        }

        private void ReleaseResources()
        {
            _factory.DisposeCollector.DisposeAll();
        }
    }
}