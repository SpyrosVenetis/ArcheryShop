using ArcheryShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class AdminController : Controller
{
    private readonly ArcheryShopContext _context;

    public AdminController(ArcheryShopContext context) => _context = context;

    // Dashboard
    public async Task<IActionResult> Index(string searchTerm)
    {
        var query = _context.Products.Include(p => p.Brand).AsQueryable();
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

    // Create (GET)
    public IActionResult Create()
    {
        ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "Name");
        ViewBag.Categories = new MultiSelectList(_context.Categories, "CategoryId", "Name");
        return View();
    }

    // Create (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, int[] selectedCategoryIds)
    {
        try
        {
            // Debugging message if things go wrong.
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("MODELSTATE INVALID");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {state.Key} | Error: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                // Create a product.
                product.Brand = null!;
                product.Categories = new List<Category>();

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Now link the product to the selected categories.
                if (selectedCategoryIds != null && selectedCategoryIds.Any())
                {
                    foreach (var id in selectedCategoryIds)
                    {
                        var cat = await _context.Categories.FindAsync(id);
                        if (cat != null) product.Categories.Add(cat);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ModelState.AddModelError("", "DATABASE CRASH: " + realError);
        }

        ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "Name", product.BrandId);
        ViewBag.Categories = new MultiSelectList(_context.Categories, "CategoryId", "Name");
        return View(product);
    }

    // Edit (GET)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        // Fetch the product along with its categories to fill the form.
        var product = await _context.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(m => m.ProductId == id);

        if (product == null) return NotFound();

        ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "Name", product.BrandId);

        // Pre-select the existing categories
        var selectedIds = product.Categories.Select(c => c.CategoryId).ToArray();
        ViewBag.Categories = new MultiSelectList(_context.Categories, "CategoryId", "Name", selectedIds);

        return View(product);
    }

    // Edit (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, int[] selectedCategoryIds)
    {
        if (id != product.ProductId) return NotFound();

        try
        {
            // Fetch the existing product along with its categories.
            var productToUpdate = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (productToUpdate == null) return NotFound();

            // Update the database fields with the form values.
            productToUpdate.Model = product.Model;
            productToUpdate.Price = product.Price;
            productToUpdate.Quantity = product.Quantity;
            productToUpdate.BrandId = product.BrandId;
            productToUpdate.Description = product.Description;
            productToUpdate.ImageLink = product.ImageLink;

            // Clears all the existing category links, then re-adds based on the selected checkboxes.
            productToUpdate.Categories.Clear();
            if (selectedCategoryIds != null)
            {
                foreach (var catId in selectedCategoryIds)
                {
                    var category = await _context.Categories.FindAsync(catId);
                    if (category != null) productToUpdate.Categories.Add(category);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Save failed: " + ex.Message);
        }

        ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "Name", product.BrandId);
        ViewBag.Categories = new MultiSelectList(_context.Categories, "CategoryId", "Name");
        return View(product);
    }

    // Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Fetch the product AND its category links.
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product != null)
            {
                // Remove the links to the categories.
                product.Categories.Clear();

                // Remove the product itself.
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            System.Diagnostics.Debug.WriteLine("DELETE FAILED: " + realError);

            return RedirectToAction(nameof(Index), new { error = "Could not delete: " + realError });
        }
    }
}