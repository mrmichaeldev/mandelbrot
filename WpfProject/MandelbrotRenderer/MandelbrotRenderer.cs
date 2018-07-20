using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ILGPU;
using ILGPU.Runtime;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageProcessor.ImageFilters
{
    internal class MandelbrotGpuFilter : IDisposable
    {
        private readonly Accelerator gpu;
        private readonly Action<Index, ArrayView<Rgba32>, ArrayView<int>, ArrayView<double>> kernel;

        public MandelbrotGpuFilter()
        {
            this.gpu = Accelerator.Create(new Context(), Accelerator.Accelerators.First(a => a.AcceleratorType == AcceleratorType.Cuda));
            this.kernel = this.gpu.LoadAutoGroupedStreamKernel<Index, ArrayView<Rgba32>, ArrayView<int>, ArrayView<double>>(ApplyKernel);
        }

        private static void ApplyKernel(
            Index index, /* The global thread index (1D in this case) */
            ArrayView<Rgba32> pixelArray, /* A view to a chunk of memory (1D in this case)*/
            ArrayView<int> imageParams,
            ArrayView<double> variables)
        {
            var x = index.X / imageParams[0];
            var y = index.X % imageParams[0];
            pixelArray[index] = Mandelbrot(x, y, imageParams[2], variables);
        }

        public Rgba32[] Apply()
        {
            const int Width = 1920;
            const int Height = 1080;
            using (MemoryBuffer<Rgba32> buffer = this.gpu.Allocate<Rgba32>(Width * Height))
            {
                const int iterations = 200;
                using (MemoryBuffer<int> imageParams = this.gpu.Allocate<int>(3))
                {
                    imageParams[0] = Width;
                    imageParams[1] = Height;
                    imageParams[2] = iterations;
                    using (MemoryBuffer<double> variables = this.gpu.Allocate<double>(10))
                    {
                        const double Zoom = 12000d;
                        const double xmin = -2.1;
                        const double ymin = -1.3;
                        const double xmax = 1;
                        const double ymax = 1.3;

                        const double panX = .4443d;
                        const double panY = .172d;

                        const double integralX = (xmax - xmin) / Width / Zoom;
                        const double integralY = (ymax - ymin) / Height / Zoom;

                        const double scrollX = (-2.2d + panX * Zoom) * (xmax - xmin) / Zoom / 2 * Height / Width;//-2d + (xmax - xmin) / Width;
                        const double scrollY = (-1.5d + panY * Zoom) * (xmax - xmin) / Zoom / 2 * Height / Width;//-1.3d + (xmax - xmin) / Height / Height;

                        variables[0] = Zoom;
                        variables[1] = xmin;
                        variables[2] = ymin;
                        variables[3] = ymax;
                        variables[4] = panX;
                        variables[5] = panY;
                        variables[6] = integralX;
                        variables[7] = integralY;
                        variables[8] = scrollX;
                        variables[9] = scrollY;

                        //buffer.CopyFrom(pixelArray, 0, Index.Zero, pixelArray.Length);


                        this.kernel(buffer.Length, buffer.View, imageParams.View, variables.View);

                        // Wait for the kernel to finish...
                        this.gpu.Synchronize();

                        return buffer.GetAsArray();
                    }
                }
            }
        }

        public static Rgba32 Mandelbrot(int arrX, int arrY, int iterations, ArrayView<double> variables)
        {
            double x1 = 0;
            double y1 = 0;
            var looper = 0;
            var x = variables[6] * arrX + variables[8];
            var y = arrY * variables[7] + variables[9];
            while (looper < iterations && Math.Sqrt((x1 * x1) + (y1 * y1)) < 2)//774 548
            {
                looper++;
                var xx = (x1 * x1) - (y1 * y1) + x;
                y1 = 2 * x1 * y1 + y;
                x1 = xx;
            }

            double val = Math.Abs(looper / iterations);
            return new Rgba32
            {
                A = 255,
                R = (byte)(255 * val * (val * 10 % 3 / 3)),
                G = (byte)(255 * (1 - val) * (val * 10 % 3 / 3)),
                B = (byte)(255 * (val * 10 % 3 / 3))
            };
        }

        public void Dispose()
        {
            this.gpu?.Dispose();
        }
    }
}
