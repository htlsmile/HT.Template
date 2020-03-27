using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HT.Template.BackEnd.Models
{
    public class UserLogin : IdentityUserLogin<Guid>
    {
        //[JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
}
