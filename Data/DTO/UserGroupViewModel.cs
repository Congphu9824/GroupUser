using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO
{
    public class UserGroupViewModel
    {
        public List<Data.Entities.User> Users { get; set; } = new List<Data.Entities.User>();
        public List<Data.Entities.Group> Groups { get; set; } = new List<Data.Entities.Group>();
    }
}
