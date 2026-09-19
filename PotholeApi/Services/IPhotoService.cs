using CloudinaryDotNet.Actions;

namespace PotholeApi.Services
{
    public interface IPhotoService
    {

        public Task<ImageUploadResult> UploadPhotoWithResultAsync(IFormFile file);
        public Task<DeletionResult> DeletePhotoAsync(string publicId);
        public Task<ImageUploadResult> UploadPhotoWithResultAsBytesAsync(byte[] ImageByts);

    }
}
