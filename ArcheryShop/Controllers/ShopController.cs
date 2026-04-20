using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcheryShop.Models;

namespace ArcheryShop.Controllers
{
    public class ShopController : Controller
    {
        // Get the database connection from program.cs and put it inside a read only variable.
        private readonly ArcheryShopContext _context;

        public ShopController(ArcheryShopContext context)
        {
            _context = context;
        }


        //asynchronous way to search for products by model or brand name, allowing multiple search terms separated by spaces.
        public async Task<IActionResult> Index(string searchTerm)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Categories);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTerms = searchTerm.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                query = query.Where(p =>
                    searchTerms.All(term =>
                        p.Model.ToLower().Contains(term) ||
                        p.Brand.Name.ToLower().Contains(term) ||
                        p.Categories.Any(c => c.Name.ToLower().Contains(term))
                        ));
            }

            var products = await query.ToListAsync();

            ViewBag.SearchTerm = searchTerm;

            return View(products);
        }

        // redirects to the details page for a specific product based on its ID, and returns a 404 Not Found response if the product does not exist
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // This is where the logic would be implemented to add a product to the shopping cart.
        public IActionResult AddToCart(int id)
        {
            return RedirectToAction("Index");
        }
    }
}