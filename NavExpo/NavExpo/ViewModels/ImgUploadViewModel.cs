using System.ComponentModel.DataAnnotations;

namespace NavExpo.ViewModels
{
    public class ImgUploadViewModel
    {
        [Required(ErrorMessage = "The Image is required")]
        public IFormFile? ImageFile { get; set; }
    }
}
