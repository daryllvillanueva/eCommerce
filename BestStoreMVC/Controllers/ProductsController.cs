using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{

    [Authorize(Roles = "admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductsController : Controller
    {

        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly int pageSize = 5;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index(int pageIndex, string? search, string? column, string? orderBy)
        {

            IQueryable<Product> query = context.Products;
            
            // search function
            if(search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }

            // sort function
            string[] validColumns = { "Id", "Name", "Brand", "Category", "Price", "CreatedAt" };
            string[] validOrderBy = { "desc", "asc" };

            if (!validColumns.Contains(column))
            {
                column = "Id";
            }

            if(!validOrderBy.Contains(orderBy))
            {
                orderBy = "asc";
            }

            if (column == "Name")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Name);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Name);
                }

                // shorter way which is the Ternary Operator
                // query = orderBy == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name); 
            }
            else if (column == "Brand")
            {
                query = orderBy == "asc" ? query.OrderBy(p => p.Brand) : query.OrderByDescending(p => p.Brand);
            }
            else if (column == "Category")
            {
                query = orderBy == "asc" ? query.OrderBy(p => p.Category) : query.OrderByDescending(p => p.Category);
            }
            else if(column == "Price")
            {
                query = orderBy == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
            }
            else if (column == "CreatedAt")
            {
                query = orderBy == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                query = orderBy == "asc" ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
            }

            // pagination function
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var products = query.ToList();

            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;
            ViewData["Search"] = search ?? "";

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if (productDto.ImageFileName == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            if(!ModelState.IsValid)
            {
                return View (productDto);
            }

            
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmm");
            newFileName += Path.GetExtension(productDto.ImageFileName!.FileName);

            string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath)) // save the image file
            {
                productDto.ImageFileName.CopyTo(stream);
            }

            // save the new product in the db
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,
            };

            context.Products.Add(product); // this will add to the table Products
            context.SaveChanges(); // save

            return RedirectToAction("Index", "Products");
        }

        public IActionResult Edit(int id) 
        { 
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");


            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);

            if (product == null) // if there's no id 
            {
                return RedirectToAction("Index", "Products");
            }

            if (!ModelState.IsValid) // if not valid there some validation errors
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
                return View(productDto);
            }

            string newFileName = product.ImageFileName;
            if (productDto.ImageFileName != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmm");
                newFileName += Path.GetExtension(productDto.ImageFileName!.FileName);

                string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath)) // save the image file
                {
                    productDto.ImageFileName.CopyTo(stream);
                }

                // delete the old image 
                string oldImageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            // update the product in the database
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();
            return RedirectToAction("Index", "Products");

        }

        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            string imageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);

            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");

        }
    }
}
