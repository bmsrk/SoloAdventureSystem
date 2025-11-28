using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace SoloAdventureSystem.LLM.Adapters
{
    public class LlamaEngine : ILLMEngine, IDisposable
    {
        private readonly ILogger<LlamaEngine>? _logger;
        private LLamaWeights? _weights;
        private LLamaContext? _context;

        public LlamaEngine(ILogger<LlamaEngine>? logger = null)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(string modelPath, int contextSize, bool useGpu, int maxThreads, IProgress<int>? progress = null)
        {
            try
            {
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = (uint)contextSize,
                    // Request GPU offload if requested. Use a large layer count to prefer GPU usage.
                    GpuLayerCount = useGpu ? 9999 : 0,
                    UseMemorymap = true,
                    UseMemoryLock = false
                };

                _logger?.LogInformation("Initializing LLama engine. Model={ModelPath}, Context={Context}, UseGPU={UseGpu}, MaxThreads={MaxThreads}", modelPath, contextSize, useGpu, maxThreads);

                try
                {
                    _logger?.LogDebug("Loading weights from {ModelPath} (attempting GPU layers: {GpuLayers})", modelPath, parameters.GpuLayerCount);

                    // Suppress noisy native and managed console output during weight loading where possible.
                    using (SuppressNativeConsoleWrites())
                    using (SuppressConsoleWrites())
                    {
                        _weights = LLamaWeights.LoadFromFile(parameters);
                        _context = _weights.CreateContext(parameters);
                    }
                }
                catch (Exception ex) when (useGpu)
                {
                    // If GPU initialization fails, log detailed diagnostics and fall back to CPU gracefully
                    _logger?.LogWarning(ex, "GPU initialization failed, attempting CPU fallback. Error: {Message}", ex.Message);

                    // Log environment diagnostics helpful for GPU failures
                    try
                    {
                        _logger?.LogWarning("Runtime OS: {OS}, ProcessArch: {Arch}", System.Environment.OSVersion, System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);
                        _logger?.LogWarning("CUDA_VISIBLE_DEVICES={CudaEnv}", Environment.GetEnvironmentVariable("CUDA_VISIBLE_DEVICES") ?? "(null)");
                        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                        if (path.Length > 200)
                            path = string.Concat(path.AsSpan(0, 200), "...");
                        _logger?.LogWarning("PATH (prefix): {PathPrefix}", path);
                    }
                    catch { /* best-effort */ }

                    // Try again with GPU disabled
                    parameters.GpuLayerCount = 0;

                    using (SuppressNativeConsoleWrites())
                    using (SuppressConsoleWrites())
                    {
                        _weights = LLamaWeights.LoadFromFile(parameters);
                        _context = _weights.CreateContext(parameters);
                    }

                    _logger?.LogInformation("Initialized LLama engine in CPU mode after GPU fallback");
                }

                if (_context != null)
                {
                    var gpuLayerCountProperty = _context.Params.GetType().GetProperty("GpuLayerCount");
                    var gpuLayerCount = gpuLayerCountProperty != null ? Convert.ToInt32(gpuLayerCountProperty.GetValue(_context.Params)) : 0;
                    var activeGpu = gpuLayerCount > 0;
                    _logger?.LogInformation("LLama engine initialized. GPU active: {GpuActive}, GpuLayerCount={GpuLayers}", activeGpu, gpuLayerCount);
                }
            }
            catch (Exception ex)
            {
                // Provide verbose diagnostics for troubleshooting native backend issues
                _logger?.LogError(ex, "Failed to initialize LLama engine: {Message}\nFull exception: {Full}", ex.Message, ex.ToString());
                throw;
            }

            await Task.CompletedTask;
        }

        public async Task<string> GenerateAsync(string prompt, int maxTokens, CancellationToken cancellationToken = default)
        {
            if (_context == null || _weights == null)
                throw new InvalidOperationException("Engine not initialized");

            var executor = new StatelessExecutor(_weights, _context.Params);
            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,
                AntiPrompts = new List<string> { "\n\n", "###", "<|end|>", "USER:", "ASSISTANT:" }
            };

            var result = new StringBuilder();
            await foreach (var text in executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                result.Append(text);
            }

            return result.ToString().Trim();
        }

        public string Generate(string prompt, int maxTokens = 150)
        {
            return GenerateAsync(prompt, maxTokens).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _context = null;
            _weights?.Dispose();
            _weights = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Attempts to suppress managed Console.Write and Console.Error during native library noise.
        /// Note: native stdout/stderr produced directly by native libraries may bypass this and require process-level redirection.
        /// This helper silences the managed Console for the scope of the returned IDisposable.
        /// </summary>
        private static IDisposable SuppressConsoleWrites()
        {
            var originalOut = Console.Out;
            var originalErr = Console.Error;
            try
            {
                Console.SetOut(TextWriter.Null);
                Console.SetError(TextWriter.Null);
            }
            catch
            {
                // ignore any issues setting console
            }

            return new DisposableAction(() =>
            {
                try
                {
                    Console.SetOut(originalOut);
                    Console.SetError(originalErr);
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Suppresses native stdout/stderr by redirecting the process standard handles to NUL on Windows.
        /// Restores original handles on dispose.
        /// </summary>
        private static IDisposable SuppressNativeConsoleWrites()
        {
            // Only supported on Windows - no-op otherwise
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new DisposableAction(() => { });

            const int STD_OUTPUT_HANDLE = -11;
            const int STD_ERROR_HANDLE = -12;
            const uint GENERIC_WRITE = 0x40000000;
            const uint FILE_SHARE_WRITE = 0x2;
            const uint OPEN_EXISTING = 3;
            const uint FILE_ATTRIBUTE_NORMAL = 0x80;

            IntPtr originalOut = GetStdHandle(STD_OUTPUT_HANDLE);
            IntPtr originalErr = GetStdHandle(STD_ERROR_HANDLE);
            IntPtr nulHandle = IntPtr.Zero;

            try
            {
                nulHandle = CreateFile("NUL", GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
                if (nulHandle != IntPtr.Zero && nulHandle != new IntPtr(-1))
                {
                    SetStdHandle(STD_OUTPUT_HANDLE, nulHandle);
                    SetStdHandle(STD_ERROR_HANDLE, nulHandle);
                }
            }
            catch
            {
                // ignore failures
            }

            return new DisposableAction(() =>
            {
                try
                {
                    if (originalOut != IntPtr.Zero && originalOut != new IntPtr(-1))
                        SetStdHandle(STD_OUTPUT_HANDLE, originalOut);
                    if (originalErr != IntPtr.Zero && originalErr != new IntPtr(-1))
                        SetStdHandle(STD_ERROR_HANDLE, originalErr);
                    if (nulHandle != IntPtr.Zero && nulHandle != new IntPtr(-1))
                        CloseHandle(nulHandle);
                }
                catch
                {
                    // ignore
                }
            });
        }

        private class DisposableAction : IDisposable
        {
            private readonly Action _action;
            private bool _disposed;
            public DisposableAction(Action action) => _action = action ?? throw new ArgumentNullException(nameof(action));
            public void Dispose()
            {
                if (_disposed) return;
                _action();
                _disposed = true;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetStdHandle(int nStdHandle, IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
