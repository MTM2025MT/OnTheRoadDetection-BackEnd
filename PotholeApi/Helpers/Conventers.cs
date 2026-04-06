namespace PotholeApi.Helpers
{
    public static class Conventers
    {
        public static IFormFile ConvertBase64ToFormFile(string base64String, string fileName)
        {
            // Base64 başlıklarını temizle (varsa) örn: "data:image/jpeg;base64,"
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            byte[] bytes = Convert.FromBase64String(base64String);
            var stream = new MemoryStream(bytes);

            // FormFile oluşturuyoruz
            var file = new FormFile(stream, 0, bytes.Length, "image_file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg" // Resim formatına göre değişebilir
            };

            return file;
        }

        public static IFormFile ConvertBytesToFormFile(byte[] image, string fileName)
        {
            // Expect raw image bytes; don't attempt base64 parsing on a byte[]
            if (image == null) throw new ArgumentNullException(nameof(image));

            var stream = new MemoryStream(image);
            stream.Position = 0;

            // FormFile oluşturuyoruz
            var file = new FormFile(stream, 0, image.Length, "image_file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg" // Resim formatına göre değişebilir
            };

            return file;
        }
    }
}
