using Microsoft.AspNetCore.Authorization;

namespace HT.Template.BackEnd
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsGranted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isGranted"></param>
        public PermissionRequirement(bool isGranted = true) => IsGranted = isGranted;
    }
}