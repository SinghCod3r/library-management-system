using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Movie
    {
        public int MovieId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Director { get; set; }

        public string Category { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation – list of transactions once movie borrowing implemented
        public ICollection<Transaction> Transactions { get; set; }
    }
}