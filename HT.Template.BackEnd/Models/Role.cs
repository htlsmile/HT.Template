using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HT.Template.BackEnd.Models
{
    public class Role : IdentityRole<Guid>
    {
        public string Description { get; set; }

        //[JsonIgnore]
        [InverseProperty(nameof(Role))]
        public virtual ICollection<UserRole> UserRoles { get; set; }

        //[JsonIgnore]
        [InverseProperty(nameof(Role))]
        public virtual ICollection<RoleClaim> RoleClaims { get; set; }
    }
}
