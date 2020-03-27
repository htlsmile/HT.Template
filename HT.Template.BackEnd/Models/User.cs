using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HT.Template.BackEnd.Models
{
    public class User : IdentityUser<Guid>
    {
        [Display(Name = "创建时间")]
        public DateTime CreationTime { get; set; } = DateTime.Now;

        //[JsonIgnore]
        [InverseProperty(nameof(User))]
        public virtual ICollection<UserClaim> Claims { get; set; }

        //[JsonIgnore]
        [InverseProperty(nameof(User))]
        public virtual ICollection<UserLogin> Logins { get; set; }

        //[JsonIgnore]
        [InverseProperty(nameof(User))]
        public virtual ICollection<UserToken> Tokens { get; set; }

        //[JsonIgnore]
        [InverseProperty(nameof(User))]
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
