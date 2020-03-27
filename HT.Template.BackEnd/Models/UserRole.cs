using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HT.Template.BackEnd.Models
{
    public class UserRole : IdentityUserRole<Guid>
    {
        //[JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        //[JsonIgnore]
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }
    }
}
