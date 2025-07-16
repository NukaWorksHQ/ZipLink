using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class PasswordDto
    {
        [Required(ErrorMessage = "Password cannot be null.")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password should be greater than 8 characters and max is 256.")]
        public required string Password { get; set; }
    }
}
