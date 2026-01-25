using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuseumSystem.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Floor { get; set; }

        public string? ImageUrl { get; set; }

        public virtual ICollection<Exhibit>? Exhibits { get; set; }
    }
}