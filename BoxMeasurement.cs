using Ozeki.Media;

namespace BoxTrackingApi.Services
{
    public class BoxMeasurement
    {
        private IRectangleDetector _rectangleDetector;

        public BoxMeasurement()
        {
            _rectangleDetector = ImageProcesserFactory.CreateRectangleDetector();
            _rectangleDetector.DetectionOccurred += RectangleDetector_DetectionOccurred;
        }

        private void RectangleDetector_DetectionOccurred(object sender, RectangleDetectedEventArgs e)
        {
            if (e.Info.Count == 0) return;

            var rectangle = e.Info.Last();
            var width = Math.Round(rectangle.Size.Width);
            var height = Math.Round(rectangle.Size.Height);

            // Calculate real-world measurements based on calibration
            CalculateRealDimensions(width, height);
        }

        private void CalculateRealDimensions(double width, double height)
        {
            // Implement your real-world dimension calculation logic here
        }
    }
}