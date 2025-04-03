using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public bool Gender { get; set; } 

        public string PhoneNumber { get; set; }
    
        public string Email { get; set; }

        public int? GroupId { get; set; }

        public virtual Group? Group { get; set; }
    }
}
