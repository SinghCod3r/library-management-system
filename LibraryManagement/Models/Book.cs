using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Book
    {
        public int BookId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Author { get; set; }
        
        [Required]
        [StringLength(20)]
        public string ISBN { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        // Navigation property
        public ICollection<Transaction> Transactions { get; set; }
    }
}