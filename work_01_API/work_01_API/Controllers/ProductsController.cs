using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using work_01_API.Models;
using work_01_API.Models.ViewModels;

namespace work_01_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ProductDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        //Get: Sales
        [HttpGet("{id}/Sales")]
        public async Task<IEnumerable<Sale>> GetSalesOfProduct(int id)
        {
            var sales=await _context.Sales.Where(s => s.ProductId == id).ToListAsync();
            return sales;
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            var existingProduct = await _context.Products
                .Include(p => p.Sales)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            _context.Entry(existingProduct).CurrentValues.SetValues(product);

            // 1. Remove deleted sales
            foreach (var existingSale in existingProduct.Sales.ToList())
            {
                if (!product.Sales.Any(s => s.SaleId == existingSale.SaleId))
                    _context.Sales.Remove(existingSale);
            }

            // 2. Update existing or Add new sales
            foreach (var saleModel in product.Sales)
            {
                var existingSale = existingProduct.Sales.FirstOrDefault(s => s.SaleId == saleModel.SaleId && s.SaleId != 0);

                if (existingSale != null)
                {
                    saleModel.ProductId = existingProduct.ProductId; // FIX: Prevent FK overwrite to 0
                    _context.Entry(existingSale).CurrentValues.SetValues(saleModel);
                }
                else
                {
                    saleModel.SaleId = 0; // Ensure it's treated as new
                    saleModel.ProductId = existingProduct.ProductId; // explicitly set FK
                    existingProduct.Sales.Add(saleModel);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        [HttpPost("Image/Upload/{id}")]
        public async Task<ActionResult<UploadResponse>> Upload(int id, IFormFile file)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound(); 
            }
            string ext = Path.GetExtension(file.FileName);
            string f = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
            string savePath = Path.Combine(_env.WebRootPath, "Images", f);
            FileStream fs = new FileStream(savePath, FileMode.Create);
            await file.CopyToAsync(fs);
            fs.Close();
            product.Picture = f;
            await _context.SaveChangesAsync();
            return new UploadResponse
            {
                FileName = f
            };
        }

        [HttpGet("Options/Size")]
        public async Task<IEnumerable<SizeOption>> GetSizeOption()
        {
            string[] names = Enum.GetNames(typeof(Size));
            List<SizeOption> result = new List<SizeOption>();
            await Task.Run(() =>
            {
                foreach (string name in names)
                {
                    Size v = (Size)Enum.Parse(typeof(Size), name);
                    result.Add(new SizeOption
                    {
                        Text = name,
                        Value = (int)v
                    });
                }
            });
            return result.ToList();
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
