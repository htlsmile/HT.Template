using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HT.Template.BackEnd.Models
{
    public class UserToken : IdentityUserToken<Guid>
    {
        //[JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
}
