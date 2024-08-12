using System.ComponentModel.DataAnnotations;

namespace TareasMVC.Models
{
    public class TokenViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email;
    }
}
