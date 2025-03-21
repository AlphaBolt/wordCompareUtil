using System;
using System.Collections.Generic;
using System.Drawing;
using ImageDiff.Analyzers;
using ImageDiff.BoundingBoxes;
using ImageDiff.Labelers;

namespace ImageDiff
{
    public class BitmapComparer : IImageComparer<Bitmap>
    {
        private LabelerTypes LabelerType { get; set; }
        private double JustNoticeableDifference { get; set; }
        private int DetectionPadding { get; set; }
        private int BoundingBoxPadding { get; set; }
        private Color BoundingBoxColor { get; set; }
        private BoundingBoxModes BoundingBoxMode { get; set; }
        private AnalyzerTypes AnalyzerType { get; set; }

        private IDifferenceLabeler Labeler { get; set; }
        private IBoundingBoxIdentifier BoundingBoxIdentifier { get; set; }
        private IBitmapAnalyzer BitmapAnalyzer { get; set; }

        public BitmapComparer(CompareOptions options = null)
        {
            if (options == null)
            {
                options = new CompareOptions();
            }
            Initialize(options);

            BitmapAnalyzer = BitmapAnalyzerFactory.Create(AnalyzerType, JustNoticeableDifference);
            Labeler = LabelerFactory.Create(LabelerType, DetectionPadding);
            BoundingBoxIdentifier = BoundingBoxIdentifierFactory.Create(BoundingBoxMode, BoundingBoxPadding);
        }

        private void Initialize(CompareOptions options)
        {
            if (options.BoundingBoxPadding < 0) throw new ArgumentException("bounding box padding must be non-negative");
            if (options.DetectionPadding < 0) throw new ArgumentException("detection padding must be non-negative");

            LabelerType = options.Labeler;
            JustNoticeableDifference = options.JustNoticeableDifference;
            BoundingBoxColor = options.BoundingBoxColor;
            DetectionPadding = options.DetectionPadding;
            BoundingBoxPadding = options.BoundingBoxPadding;
            BoundingBoxMode = options.BoundingBoxMode;
            AnalyzerType = options.AnalyzerType;
        }

        public Bitmap Compare(Bitmap firstImage, Bitmap secondImage)
        {
            if (firstImage == null) throw new ArgumentNullException("firstImage");
            if (secondImage == null) throw new ArgumentNullException("secondImage");

            // Pad images with white pixels if they are not the same size
            if (firstImage.Width != secondImage.Width || firstImage.Height != secondImage.Height)
            {
                // Determine the maximum dimensions
                int maxWidth = Math.Max(firstImage.Width, secondImage.Width);
                int maxHeight = Math.Max(firstImage.Height, secondImage.Height);

                firstImage = PadImage(firstImage, maxWidth, maxHeight);
                secondImage = PadImage(secondImage, maxWidth, maxHeight); ;
            }

            var differenceMap = BitmapAnalyzer.Analyze(firstImage, secondImage);
            var differenceLabels = Labeler.Label(differenceMap);
            var boundingBoxes = BoundingBoxIdentifier.CreateBoundingBoxes(differenceLabels);
            var differenceBitmap = CreateImageWithBoundingBoxes(secondImage, boundingBoxes);
            return differenceBitmap;
        }

        private Bitmap CreateImageWithBoundingBoxes(Bitmap secondImage, IEnumerable<Rectangle> boundingBoxes)
        {
            var differenceBitmap = secondImage.Clone() as Bitmap;
            if (differenceBitmap == null) throw new Exception("Could not copy secondImage");

            using (var g = Graphics.FromImage(differenceBitmap))
            {
                var pen = new Pen(BoundingBoxColor);
                foreach (var boundingRectangle in boundingBoxes)
                {
                    g.DrawRectangle(pen, boundingRectangle);
                }
            }
            return differenceBitmap;
        }

        private Bitmap PadImage(Bitmap image, int width, int height)
        {
            // Create a new bitmap with the specified dimensions
            Bitmap padded = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(padded))
            {
                // Fill with white background
                g.Clear(Color.White);
                // Draw the original image using the calculated offsets
                int xOffset = (width - image.Width) / 2;
                int yOffset = (height - image.Height) / 2;
                g.DrawImage(image, xOffset, yOffset, image.Width, image.Height);
            }
            return padded;
        }

    }
}
