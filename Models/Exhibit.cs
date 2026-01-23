using System.ComponentModel.DataAnnotations;

namespace MuseumSystem.Models
{
    public class Exhibit
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        
        [Required]
        public int ArtistId { get; set; }
        
        public Artist? Artist { get; set; }
    }
}