using HT.Template.BackEnd.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace HT.Template.BackEnd
{
    public static class EntityFrameworkCoreExtensions
    {
        /// <summary>
        /// 自动添加Model
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static ModelBuilder AddModels(this ModelBuilder modelBuilder, Assembly assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            var entityTypes = assembly.GetTypes()
                .Where(type => !type.IsAbstract && typeof(Entity).IsAssignableFrom(type));
            foreach (var entityType in entityTypes)
            {
                if (modelBuilder.Model.FindEntityType(entityType) == null)
                {
                    modelBuilder.Model.AddEntityType(entityType);
                }
            }
            return modelBuilder;
        }

        /// <summary>
        /// 表名和字段名小写
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <returns></returns>
        public static ModelBuilder ToLower(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var currentTableName = modelBuilder.Entity(entity.Name).Metadata.GetTableName();
                modelBuilder.Entity(entity.Name).ToTable(currentTableName.ToLower());
                foreach (var property in entity.GetProperties())
                {
                    modelBuilder.Entity(entity.Name).Property(property.Name).HasColumnName(property.Name.ToLower());
                }
            }
            return modelBuilder;
        }

        /// <summary>
        /// 生成Model时添加注释
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static ModelBuilder AddComment(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.ClrType?.IsDefined(typeof(DisplayAttribute)) == true)
                {
                    entityType.SetComment(entityType.ClrType?.GetCustomAttribute<DisplayAttribute>().Name);
                }
                foreach (var property in entityType.GetProperties())
                {
                    if (property.PropertyInfo?.IsDefined(typeof(DisplayAttribute)) == true)
                    {
                        property.SetComment(property.PropertyInfo?.GetCustomAttribute<DisplayAttribute>().Name);
                    }
                }
            }
            return modelBuilder;
        }
    }
}
