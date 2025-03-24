using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Filter
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            return value;
        }
        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
    }
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }
    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }
        }
    }
    class GrayScale : Filters
    {
        protected int Intensety(Color sourceColor)
        {
            float coefR = 0.299f;
            float coefG = 0.587f;
            float coefB = 0.114f;
            int intensety = (int)(coefR * sourceColor.R + coefG * sourceColor.G + coefB * sourceColor.B);
            return intensety;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int tmp = Intensety(sourceColor);
            //intensety = coefR * sourceColor.R + coefG * sourceColor.G + coefB * sourceColor.B;
            Color resultColor = Color.FromArgb(tmp, tmp, tmp);
            return resultColor;
        }
    }
    //class Sepia : GrayScale //Не произойдет ли вызов Calculate для  GrayScale по умолчанию?
    //{
    //    private int k = 10;
    //    protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    //    {
    //        Color sourceColor = sourceImage.GetPixel(x, y);
    //        int tmp = Intensety(sourceColor);
    //        int resR = Clamp(tmp + 2 * k, 0, 255);
    //        int resG = Clamp(tmp + (int)(0.5 * k),0 ,255);
    //        int resB = Clamp(tmp - 1 * k,0, 255);
    //        Color resultColor = Color.FromArgb(resR, resG, resB);
    //        return resultColor;
    //    }
    //}
    class Sepia : Filters
    {
        private int k = 10;
        protected int Intensety(Color sourceColor)
        {
            float coefR = 0.299f;
            float coefG = 0.587f;
            float coefB = 0.114f;
            int intensety = (int)(coefR * sourceColor.R + coefG * sourceColor.G + coefB * sourceColor.B);
            return intensety;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int tmp = Intensety(sourceColor);
            int resR = Clamp(tmp + 2 * k, 0, 255);
            int resG = Clamp(tmp + (int)(0.5 * k), 0, 255);
            int resB = Clamp(tmp - 1 * k, 0, 255);
            Color resultColor = Color.FromArgb(resR, resG, resB);
            return resultColor;
        }
    }
    class IncreasingBrightness : Filters
    {
        protected int k = 20;
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int resR = Clamp(sourceColor.R + k, 0, 255);
            int resG = Clamp(sourceColor.G + k, 0, 255);
            int resB = Clamp(sourceColor.B + k, 0, 255);
            Color resultColor = Color.FromArgb(resR, resG, resB);
            return resultColor;
        }
    }
    class RightShift : Filters
    {
        protected int k = 50;
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color resultColor = Color.Black;

            if (x >= k && x < sourceImage.Width)
            {
                resultColor = sourceImage.GetPixel(x - k, y);
                return resultColor;
            }
            else
            {
                return Color.Black;

            }
        }
    }
    class Embossing : MatrixFilter
    {
        protected int Intensety(Color sourceColor)
        {
            float coefR = 0.299f;
            float coefG = 0.587f;
            float coefB = 0.114f;
            int intensety = Clamp((int)(coefR * sourceColor.R + coefG * sourceColor.G + coefB * sourceColor.B), 0, 255);
            return intensety;
        }
        public Embossing()
        {
            const int sizeX = 3;
            const int sizeY = 3;
            kernel = new float[sizeX, sizeY] { { 0.0f, 1.0f, 0.0f }, { -1.0f, 0.0f, 1.0f }, { 0.0f, -1.0f, 0.0f } };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {

            Color step1Color = base.calculateNewPixelColor(sourceImage, x, y);
            int resR = Clamp(step1Color.R + 100, 0, 255);
            int resG = Clamp(step1Color.G + 100, 0, 255);
            int resB = Clamp(step1Color.B + 100, 0, 255);
            Color step2 = Color.FromArgb(resR, resG, resB);
            return Color.FromArgb(Intensety(step2), Intensety(step2), Intensety(step2));
        }

    }

    class MotionBlur : MatrixFilter
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return base.calculateNewPixelColor(sourceImage, x, y);
        }
        public MotionBlur()
        {
            int sizeX = 9;
            int sizeY = 9;

            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    if (i == j)
                    {
                        kernel[i, j] = 1.0f / (float)(sizeY);
                    }
                    else
                    {
                        kernel[i, j] = 0.0f;
                    }
                }
            }
        }
    }
    class GrayWorld : Filters
    {
        protected static bool key = true;
        protected static float coefR, coefG, coefB;
        protected static void calculatecoef(Bitmap sourceImage)
        {
            int midColorR = 0;
            int midColorG = 0;
            int midColorB = 0;
            int N = sourceImage.Width * sourceImage.Height;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color tmpColor = sourceImage.GetPixel(i, j);
                    midColorR += (int)tmpColor.R;
                    midColorG += (int)tmpColor.G;
                    midColorB += (int)tmpColor.B;
                }
            }

            float avgR = (float)midColorR / N;
            float avgG = (float)midColorG / N;
            float avgB = (float)midColorB / N;

            float avgGray = (avgR + avgG + avgB) / 3.0f;

            coefR = avgGray / avgR;
            coefG = avgGray / avgG;
            coefB = avgGray / avgB;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            if (key == true)
            {
                calculatecoef(sourceImage);
                key = false;
            }
            Color sourceColor = sourceImage.GetPixel(x, y);

            Color resultColor = Color.FromArgb(
                Clamp((int)((int)sourceColor.R * coefR), 0, 255),
                Clamp((int)((int)sourceColor.G * coefG), 0, 255),
                Clamp((int)((int)sourceColor.B * coefB), 0, 255)
                );
            return resultColor;
        }

    }
    class LinearStretching : Filters
    {
        private static bool key = true;
        private static int maxR = 0, minR = 255, maxG = 0, minG = 255, maxB = 0, minB = 255;
        private static float coefR = 0, coefG = 0, coefB = 0;
        protected static void calculatecoef(Bitmap sourceImage)
        {

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color tmpColor = sourceImage.GetPixel(i, j);
                    if ((int)tmpColor.R > maxR)
                    {
                        maxR = (int)tmpColor.R;
                    }
                    if ((int)tmpColor.R < minR)
                    {
                        minR = (int)tmpColor.R;
                    }
                    if ((int)tmpColor.G > maxG)
                    {
                        maxG = (int)tmpColor.G;
                    }
                    if ((int)tmpColor.G < minG)
                    {
                        minG = (int)tmpColor.G;
                    }

                    if ((int)tmpColor.B > maxB)
                    {
                        maxB = (int)tmpColor.B;
                    }
                    if ((int)tmpColor.B < minB)
                    {
                        minB = (int)tmpColor.B;
                    }

                }
            }
            coefR = (maxR == minR) ? 1.0f : 255.0f / (float)(maxR - minR);
            coefG = (maxG == minG) ? 1.0f : 255.0f / (float)(maxG - minG);
            coefB = (maxB == minB) ? 1.0f : 255.0f / (float)(maxB - minB);

        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            if (key == true)
            {
                calculatecoef(sourceImage);
                key = false;
            }
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(
                Clamp((int)((sourceColor.R - minR) * coefR), 0, 255),
                Clamp((int)((sourceColor.G - minG) * coefG), 0, 255),
                Clamp((int)((sourceColor.B - minB) * coefB), 0, 255)
                );
            return resultColor;
        }
    }
    class PerfectReflector : Filters
    {
        private static bool key = true;
        private static int maxR = 0, maxG = 0, maxB = 0;
        private static float coefR = 0, coefG = 0, coefB = 0;
        protected static void calculatecoef(Bitmap sourceImage)
        {

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color tmpColor = sourceImage.GetPixel(i, j);
                    if ((int)tmpColor.R > maxR)
                    {
                        maxR = (int)tmpColor.R;
                    }
                    if ((int)tmpColor.G > maxG)
                    {
                        maxG = (int)tmpColor.G;
                    }
                    if ((int)tmpColor.B > maxB)
                    {
                        maxB = (int)tmpColor.B;
                    }
                }
            }
            coefR = (maxR == 0) ? 1.0f : 255.0f / (float)(maxR);
            coefG = (maxG == 0) ? 1.0f : 255.0f / (float)(maxG);
            coefB = (maxB == 0) ? 1.0f : 255.0f / (float)(maxB);

        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            if (key == true)
            {
                calculatecoef(sourceImage);
                key = false;
            }
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(
                Clamp((int)((sourceColor.R) * coefR), 0, 255),
                Clamp((int)((sourceColor.G) * coefG), 0, 255),
                Clamp((int)((sourceColor.B) * coefB), 0, 255)
                );
            return resultColor;
        }
    }
    class Extension : MatrixFilter
    {
        public Extension()
        {
            const int size = 3;
            kernel = new float[size, size]
            {
                { 0.0f,1.0f,0.0f},
                { 1.0f,1.0f,1.0f},
                { 0.0f,1.0f,0.0f}
            };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            int max = 0;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int pixelX = x + k;
                    int pixelY = y + l;

                    if (pixelX >= 0 && pixelY >= 0 && pixelX < sourceImage.Width && pixelY < sourceImage.Height)//чекаем выход за пределы изображения
                    {
                        if (kernel[k + radiusX, l + radiusY] == 1)//приводим индексы к нетрицательному виду
                        {
                            Color pixelColor = sourceImage.GetPixel(pixelX, pixelY);
                            int intensity = pixelColor.R + pixelColor.G + pixelColor.B;
                            max = Math.Max(max, intensity);
                        }

                    }
                }
            }
            return Color.FromArgb(Clamp(max, 0, 255), Clamp(max, 0, 255), Clamp(max, 0, 255));
        }
    }
    class Contraction : MatrixFilter
    {
        public Contraction()
        {
            const int size = 3;
            kernel = new float[size, size]
            {
                { 0.0f,1.0f,0.0f},
                { 1.0f,1.0f,1.0f},
                { 0.0f,1.0f,0.0f}
            };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            int minR = 255;
            int minG = 255; 
            int minB = 255; 

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int pixelX = x + k;
                    int pixelY = y + l;

                    if (pixelX >= 0 && pixelY >= 0 && pixelX < sourceImage.Width && pixelY < sourceImage.Height)//чекаем выход за пределы изображения
                    {
                        if ((int)kernel[k + radiusX, l + radiusY] != 1)//приводим индексы к нетрицательному виду
                        {
                            Color pixelColor = sourceImage.GetPixel(pixelX, pixelY);

                            minR = Math.Min(minR, pixelColor.R);
                            minG = Math.Min(minG, pixelColor.G);
                            minB = Math.Min(minB, pixelColor.B);
                        }
                    }
                }
            }
            return Color.FromArgb(Clamp(minR, 0, 255), Clamp(minG, 0, 255), Clamp(minB, 0, 255));
        }
    }

    class MedianFilter : MatrixFilter
    {
        private static int[ ] intensety = new int[9];
        private static int tmp=0;
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {

            Color resultColor = Color.Black;
            for (int l = -1; l <= 1; l++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color sourceColor = sourceImage.GetPixel(idX, idY);
                    int resR = (int)sourceColor.R;
                    int resG = (int)sourceColor.G;
                    int resB = (int)sourceColor.B;

                    intensety[tmp]=(resR+resG+resB)/3;
                    tmp++;
                }
            }
            tmp = 0;
            Array.Sort(intensety);
            resultColor = Color.FromArgb(Clamp(intensety[4],0,255), Clamp(intensety[4],0,255), Clamp(intensety[4],0,255));
            return resultColor;
        }
    }
    class SobelFilter: MatrixFilter
    {
        protected static float[,] kernelGX = new float[3, 3]
        {
            {-1, 0, 1 },
            {-2, 0, 2 },
            {-1, 0, 1 }
        };
        protected static float[,] kernelGY = new float[3, 3]
        {
            {-1,-2, -1},
            { 0, 0, 0 },
            { 1, 2, 1 }
        };
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float GXColorR = 0;
            float GXColorG = 0;
            float GXColorB = 0;

            float GYColorR = 0;
            float GYColorG = 0;
            float GYColorB = 0;

            for (int l = -1; l <= 1; l++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color tmp = sourceImage.GetPixel(idX, idY);

                    GXColorR += tmp.R * kernelGX[k + 1, l + 1];
                    GXColorG += tmp.G * kernelGX[k + 1, l + 1];
                    GXColorB += tmp.B * kernelGX[k + 1, l + 1];

                    GYColorR += tmp.R * kernelGY[k + 1, l + 1];
                    GYColorG += tmp.G * kernelGY[k + 1, l + 1];
                    GYColorB += tmp.B * kernelGY[k + 1, l + 1];
                }
            }

            int red = (int)Math.Sqrt(GXColorR * GXColorR + GYColorR * GYColorR);
            int green = (int)Math.Sqrt(GXColorG * GXColorG + GYColorG * GYColorG);
            int blue = (int)Math.Sqrt(GXColorB * GXColorB + GYColorB * GYColorB);

            red = Clamp(red, 0, 255);
            green = Clamp(green, 0, 255);
            blue = Clamp(blue, 0, 255);

            return Color.FromArgb(red, green, blue);
        }
    }
    class SharrFilter: MatrixFilter
    {
        protected static float[,] kernelGX = new float[3, 3]
            {
                {-3, 0, 3},
                {-10, 0,10},
                { -3, 0, 3}
            };
        protected static float[,] kernelGY = new float[3, 3]
        {
                {-3, -10, -3 },
                {0, 0, 0 },
                { 3, 10, 3 }
        };

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float GXColorR = 0;
            float GXColorG = 0;
            float GXColorB = 0;

            float GYColorR = 0;
            float GYColorG = 0;
            float GYColorB = 0;

            Color resultColor = Color.Black;

            for (int l = -1; l <= 1; l++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color tmp = sourceImage.GetPixel(idX, idY);

                    GXColorR += tmp.R * kernelGX[k + 1, l + 1];
                    GXColorG += tmp.G * kernelGX[k + 1, l + 1];
                    GXColorB += tmp.B * kernelGX[k + 1, l + 1];

                    GYColorR += tmp.R * kernelGY[k + 1, l + 1];
                    GYColorG += tmp.G * kernelGY[k + 1, l + 1];
                    GYColorB += tmp.B * kernelGY[k + 1, l + 1];
                }
            }
            int red = (int)Math.Sqrt(GXColorR * GXColorR + GYColorR * GYColorR);
            int green = (int)Math.Sqrt(GXColorG * GXColorG + GYColorG * GYColorG);
            int blue = (int)Math.Sqrt(GXColorB * GXColorB + GYColorB * GYColorB);

            red = Clamp(red, 0, 255);
            green = Clamp(green, 0, 255);
            blue = Clamp(blue, 0, 255);

            return Color.FromArgb(red, green, blue);
        }
    }
}

