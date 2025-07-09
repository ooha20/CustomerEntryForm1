using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DEMO.Models
{
    public class Customer
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Zip Code")]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Enter a valid zip code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
        public string Phone { get; set; } = string.Empty;


        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;


        [NotMapped]
        [Compare("Email", ErrorMessage = "Emails do not match")]
        [Display(Name = "Confirm Email")]
        public string ConfirmEmail { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "Upload Image")]
        [Required]
        public IFormFile? Image { get; set; }
        public byte[]? ImageData { get; set; }

        public string? ImagePath { get; set; }
        
    }
}


