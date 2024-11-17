using IronOcr;
using System.Linq;
using System.Threading.Tasks;

namespace BoxTrackingApi.Services
{
    public class LabelReader
    {
        private IronTesseract _ocr;

        public LabelReader()
        {
            _ocr = new IronTesseract();
            _ocr.Configuration.ReadBarCodes = true; // Enable barcode reading
        }

        public async Task<string> ReadLabel(byte[] imageData)
        {
            using (var input = new OcrInput())
            {
                input.AddImage(imageData);
                var result = _ocr.Read(input);

                // Return both text and barcode data
                return $"Text: {result.Text}\nBarcodes: {string.Join(", ", result.Barcodes.Select(b => b.Value))}";
            }
        }
    }
}