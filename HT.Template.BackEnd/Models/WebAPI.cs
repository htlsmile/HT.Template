using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HT.Template.BackEnd.Models
{
    public class WebAPI : Entity
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        [Required]
        [Display(Name = "接口名称")]
        public string Name { get; set; }

        /// <summary>
        /// 接口描述
        /// </summary>
        [Required]
        [Display(Name = "接口描述")]
        public string Description { get; set; }

        /// <summary>
        /// HTTP请求方法
        /// </summary>
        [Required]
        [Display(Name = "HTTP请求方法")]
        public string Method { get; set; }

        /// <summary>
        /// 路由
        /// </summary>
        [Required]
        [Display(Name = "路由")]
        public string Path { get; set; }

    }
}
