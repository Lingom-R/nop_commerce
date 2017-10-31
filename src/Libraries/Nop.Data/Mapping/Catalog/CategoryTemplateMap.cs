using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class CategoryTemplateMap : NopEntityTypeConfiguration<CategoryTemplate>
    {
        public override void Configure(EntityTypeBuilder<CategoryTemplate> builder)
        {
            base.Configure(builder);
            builder.ToTable("CategoryTemplate");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(400);
            builder.Property(p => p.ViewPath).IsRequired().HasMaxLength(400);
        }
    }
}