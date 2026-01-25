using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuseumSystem.Models
{
    public class Artist
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Bio { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        
        public List<Exhibit>? Exhibits { get; set; } = new();
    }
}