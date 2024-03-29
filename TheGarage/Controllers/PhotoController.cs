using Microsoft.AspNetCore.Mvc;


namespace TheGarage.Controllers
{
    [Route("vehicles/photos")]
    [ApiController]
    public class PhotoController(IPhotoStorage photoStorage) : ControllerBase
    {
        private const string MultipartContentType = "multipart/form-data";
        
        [HttpPost(Name = "UploadPhoto")]
        public async Task<IActionResult> UploadPhoto()
        {
            if (!IsMultipartContentType(Request.ContentType))
            {
                return BadRequest("Request is not a multipart request");
            }

            var formCollection = await Request.ReadFormAsync();
            var file = formCollection.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty");
            }

            
            await using (var stream = file.OpenReadStream())
            {
                var uri = await photoStorage.StorePhoto(file.FileName, stream);

                //Response.Headers.Location = uri;

                return Created(uri, uri);
            }
        }

        private bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType) &&
                   contentType.IndexOf(MultipartContentType, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
