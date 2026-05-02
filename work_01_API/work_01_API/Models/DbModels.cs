using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace work_01_API.Models
{
    public enum Size
    {
        XL = 1, L, M, S
    }
    public class Product
    {
        public int ProductId { get; set; }
        [Required, StringLength(50)]
        public string ProductName { get; set; } = default!;
        [Required, EnumDataType(typeof(Size))]
        public Size Size { get; set; }
        [Required, Column(TypeName = "money")]
        public decimal Price { get; set; }
        [StringLength(150)]
        public string Picture { get; set; } = default!;
        public bool? OnSale { get; set; }

        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
    public class Sale
    {
        public int SaleId { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }
    }
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {

        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    ProductName = "Product one",
                    Price = 90.00M,
                    Size = Size.XL,
                    Picture = "1.jpg"
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Product two",
                    Price = 90.00M,
                    Size = Size.L,
                    Picture = "2.jpg"
                });
            modelBuilder.Entity<Sale>().HasData(
                new Sale
                {
                    SaleId = 1,
                    ProductId = 1,
                    Date = DateTime.Today.AddDays(-10),
                    Quantity = 120
                },
                new Sale
                {
                    SaleId = 2,
                    ProductId = 1,
                    Date = DateTime.Today.AddDays(-20),
                    Quantity = 50
                },
                 new Sale
                 {
                     SaleId = 3,
                     ProductId = 2,
                     Date = DateTime.Today.AddDays(-20),
                     Quantity = 150
                 });
        }
    }
}
