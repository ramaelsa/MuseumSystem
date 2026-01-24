using System;
using System.ComponentModel.DataAnnotations;

namespace MuseumSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        public string? UserId { get; set; } 

        [Required(ErrorMessage = "Visitor name is required")]
        [Display(Name = "Visitor Name")]
        public string VisitorName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; } = DateTime.Now;

        [Range(0, 500)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}