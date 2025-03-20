﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
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
            for( int i=0; i< sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                for( int j=0; j< sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
    }
    class InvertFilter:Filters
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
            for( int l=-radiusY; l<= radiusY; l++)
            {
                for( int k= -radiusX; k<= radiusX; k++)
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
            for( int i=0; i< sizeX; i++)
            {
                for( int j=0; j< sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }
        }
    }
    class GrayScale :Filters
    {
        protected int Intensety(Color sourceColor)
        {
            float coefR = 0.299f;
            float coefG = 0.587f;
            float coefB = 0.114f;
            int intensety= (int)(coefR * sourceColor.R + coefG * sourceColor.G + coefB * sourceColor.B);
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
    class IncreasingBrightness: Filters
    {
        protected int k =20;
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int resR = Clamp(sourceColor.R+ k, 0, 255);
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

            if(x>=k && x < sourceImage.Width)
            {
                resultColor = sourceImage.GetPixel(x - k, y);
            }
            return resultColor;
        }
    }
}
