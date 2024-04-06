using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeFirstExample.Domain.Entities
{
    public class StrongEntity<TEntity> : BaseEntity<TEntity> where TEntity : class
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual Guid Id { get; set; }

        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
        }
    }
}
