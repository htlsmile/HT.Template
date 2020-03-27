using HT.Template.BackEnd.DTOs;
using HT.Template.BackEnd.Hubs;
using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HT.Template.BackEnd.Controllers
{
    public class AccountController : AppControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AccountController(AppDbContext context,
                                 IHubContext<ApplicationHub, IApplicationHubClient> hubContext,
                                 UserManager<User> userManager,
                                 SignInManager<User> signInManager) : base(context, hubContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(Register))]
        public async Task<IActionResult> Register(UserDTO dto)
        {
            var result = await userManager.CreateAsync(new User { UserName = dto.UserName }, dto.Password);
            var user = await userManager.FindByNameAsync(dto.UserName);
            return result.Succeeded ? Ok(new APIResult(true, GetJwtToken(user))) : Ok(new APIResult(false, result.Errors.ToString()));
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost(nameof(Login))]
        public async Task<IActionResult> Login(UserDTO dto)
        {
            var user = await userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                return Ok(new APIResult(false, $"{dto.UserName}不存在"));
            }
            var result = await signInManager.PasswordSignInAsync(user, dto.Password, false, true);
            if (result.IsLockedOut)
            {
                return Ok(new APIResult(false, $"{dto.UserName}已被锁定"));
            }
            else if (result.IsNotAllowed)
            {
                return Ok(new APIResult(false, $"{dto.UserName}禁止登录"));
            }
            else if (result.RequiresTwoFactor)
            {
                return Ok(new APIResult(false, $"{dto.UserName}需要两步验证"));
            }
            else if (result.Succeeded)
            {
                return Ok(new APIResult(true, GetJwtToken(user)));
            }
            else
            {
                return Ok(new APIResult(false, $"{dto.UserName}密码错误"));
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(Logout))]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok(new APIResult(true));
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost(nameof(ChangePassword))]
        public async Task<IActionResult> ChangePassword(UserDTO dto)
        {
            var user = await userManager.FindByNameAsync(dto.UserName);
            var result = await userManager.ChangePasswordAsync(user, dto.Password, dto.NewPassword);
            return result.Succeeded ? Ok(new APIResult(true, GetJwtToken(user))) : Ok(new APIResult(false, result.Errors.ToString()));
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(ResetPassword))]
        public async Task<IActionResult> ResetPassword(UserDTO dto)
        {
            var user = await userManager.FindByNameAsync(dto.UserName);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, dto.Password);
            return result.Succeeded ? Ok(new APIResult(true, GetJwtToken(user))) : Ok(new APIResult(false, result.Errors.ToString()));
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        [HttpDelete(nameof(Delete))]
        public async Task<IActionResult> Delete(UserDTO dto)
        {
            var user = await userManager.FindByNameAsync(dto.UserName);
            if (!await userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Ok(new APIResult(false, "密码错误"));
            }
            var result = await userManager.DeleteAsync(user);
            return Ok(new APIResult(result.Succeeded, result.Errors.ToString()));
        }

        private string GetJwtToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.UserName)
            });
            var now = DateTime.Now;
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(JwtExtensions.SecurityKey), SecurityAlgorithms.HmacSha256);
            var token = handler.CreateJwtSecurityToken(
                issuer: JwtExtensions.Issuer,
                audience: JwtExtensions.Audience,
                subject: claimsIdentity,
                notBefore: now,
                expires: now.AddMinutes(30),
                issuedAt: now,
                signingCredentials: signingCredentials);
            return handler.WriteToken(token);
        }

    }
}
