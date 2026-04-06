using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using PotholeApi.Helpers;

namespace PotholeApi.Services
{
    public class PhotoService: IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> config  ) { 
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }
        public async Task<ImageUploadResult> UploadPhotoWithResultAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            // CHECK 1: Is the file actually there?
            if (file == null || file.Length == 0)
            {
                // This is likely where your issue is if the Postman file isn't reaching here
                throw new Exception("File is empty or null");
            }

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // CHECK 2: Did Cloudinary return an error?
            if (uploadResult.Error != null)
            {
                // This will reveal if your API Keys or Cloud Name are wrong
                throw new Exception(uploadResult.Error.Message);
            }

            return uploadResult;
        }
        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await  _cloudinary.DestroyAsync(deleteParams);
            return result;

        }

        public async Task<ImageUploadResult> UploadPhotoWithResultAsBytesAsync(byte[] ImageByts)
        {
            string finalImageUrl = "";
            if (ImageByts.Length!=0)
            {
                // 1. Base64 string'i IFormFile'a çeviriyoruz
                var formFile = Conventers.ConvertBytesToFormFile(ImageByts, "upload.jpg");
                if (formFile == null)
                {
                    throw new Exception( "Failed to process image. The Base64 string is invalid or corrupted.");
                }
                // 2. Artık elinde IFormFile var, servise gönderebilirsin
                var photoResult = await UploadPhotoWithResultAsync(formFile);

                // 3. Cloudinary/Servis'ten dönen gerçek URL'i alıyoruz

                return photoResult;
            }
            return null;
        }
    }
}
