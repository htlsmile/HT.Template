using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HT.Template.BackEnd.Models
{
    public class RoleClaim : IdentityRoleClaim<Guid>
    {
        //[JsonIgnore]
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }
    }
}
