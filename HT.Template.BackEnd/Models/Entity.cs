using System;
using System.ComponentModel.DataAnnotations;

namespace HT.Template.BackEnd.Models
{
    [Display(Name = "模型基类")]
    public abstract class Entity
    {
        [Required]
        [Display(Name = nameof(Id))]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Display(Name = "创建者Id")]
        public Guid? CreatorId { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreationTime { get; set; }

        [Display(Name = "最后修改者Id")]
        public Guid? LastModifierId { get; set; }

        [Display(Name = "最后修改时间")]
        public DateTime? LastModificationTime { get; set; }

        [Display(Name = "是否已删除")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "删除者Id")]
        public Guid? DeleterId { get; set; }

        [Display(Name = "删除时间")]
        public DateTime? DeletionTime { get; set; }
    }
}
