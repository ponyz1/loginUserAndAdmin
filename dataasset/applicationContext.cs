using Microsoft.EntityFrameworkCore;
using TodoApi.model;

namespace TodoApi.dataasset{
    public class applicationContext:DbContext{
        public applicationContext(DbContextOptions<applicationContext> options) : base(options){
            
        }
        protected override void OnModelCreating(ModelBuilder builder){
            builder.Entity<Product>(entity => {
                entity.ToTable("product");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();

            });
            base.OnModelCreating(builder);

        }
        public DbSet<Product> Products { get; set; }
    }


}