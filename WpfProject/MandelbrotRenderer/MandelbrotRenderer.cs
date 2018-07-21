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

        public Rgba32[] Apply(int iterations, int width, int height, double panX, double panY, double zoom)
        {
            using (MemoryBuffer<Rgba32> buffer = this.gpu.Allocate<Rgba32>(width * height))
            {
                //int iterations = (int)(255 * zoom);
                using (MemoryBuffer<int> imageParams = this.gpu.Allocate<int>(3))
                {
                    imageParams[0] = width;
                    imageParams[1] = height;
                    imageParams[2] = iterations;
                    using (MemoryBuffer<double> variables = this.gpu.Allocate<double>(10))
                    {
                        const double xmin = -2.1;
                        const double xmax = 1;
                        const double ymin = -1.3;
                        const double ymax = 1.3;

                        double integralX = (xmax - xmin) / width / zoom;
                        double integralY = (ymax - ymin) / height / zoom;

                        double scrollX = (-2.2d + panX * zoom) * (xmax - xmin) / zoom / 2 * height / width;//-2d + (xmax - xmin) / Width;
                        double scrollY = (-1.5d + panY * zoom) * (xmax - xmin) / zoom / 2 * height / width;//-1.3d + (xmax - xmin) / Height / Height;

                        variables[0] = zoom;
                        variables[1] = xmin;
                        variables[2] = ymin;
                        variables[3] = ymax;
                        variables[4] = panX;
                        variables[5] = panY;
                        variables[6] = integralX;
                        variables[7] = integralY;
                        variables[8] = scrollX;
                        variables[9] = scrollY;

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

            //double val = Math.Abs(looper / iterations);
            var val = (double)looper / (double)iterations * 255;
            return new Rgba32
            {
                A = 255,
                R = (byte)(val),
                G = (byte)(val),
                B = (byte)(val)
            };
        }

        public void Dispose()
        {
            this.gpu?.Dispose();
        }
    }
}
