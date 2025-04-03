using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Group
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string GroupCode { get; set; }

        [Required]
        [StringLength(100)]
        public string GroupName { get; set; }

        public int? ParentGroupId { get; set; }

        [ForeignKey("ParentGroupId")]
        public virtual Group? ParentGroup { get; set; } // Điều hướng đến nhóm cha
        public virtual ICollection<Group>? ChildGroups { get; set; } = new HashSet<Group>(); // group con
        public virtual ICollection<User>? Users { get; set; } = new HashSet<User>();
    }
}
