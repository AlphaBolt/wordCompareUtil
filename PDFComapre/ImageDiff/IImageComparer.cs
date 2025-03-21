﻿using System.Drawing;

namespace ImageDiff
{
    public interface IImageComparer<T> where T : Image
    {
        T Compare(T firstImage, T secondImage);
    }
}