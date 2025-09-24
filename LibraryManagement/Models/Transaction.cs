using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        
        public int BookId { get; set; }
        
        public int MemberId { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Fine { get; set; }
        
        // Navigation properties
        public Book Book { get; set; }
        public Member Member { get; set; }
    }
}