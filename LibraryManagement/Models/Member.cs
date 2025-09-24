using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(50)]
        public string MembershipType { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }
        
        // Navigation property
        public ICollection<Transaction> Transactions { get; set; }
    }
}