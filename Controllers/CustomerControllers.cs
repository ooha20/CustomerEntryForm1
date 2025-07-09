using System;
using System.IO;
using System.Linq;
using DEMO.Data;
using DEMO.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DEMO.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            IWebHostEnvironment environment,
            ApplicationDbContext db,
            ILogger<CustomerController> logger)
        {
            _environment = environment;
            _db = db;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Edit()
        {
            _logger.LogInformation("User clicked Edit at {Time}", DateTime.UtcNow);

            var model = new Customer
            {
                FirstName = TempData["FirstName"] as string ?? string.Empty,
                LastName = TempData["LastName"] as string ?? string.Empty,
                Address = TempData["Address"] as string ?? string.Empty,
                City = TempData["City"] as string ?? string.Empty,
                State = TempData["State"] as string ?? string.Empty,
                ZipCode = TempData["ZipCode"] as string ?? string.Empty,
                Phone = TempData["Phone"] as string ?? string.Empty,
                Email = TempData["Email"] as string ?? string.Empty,
                ConfirmEmail = TempData["ConfirmEmail"] as string ?? string.Empty
                // Note: file inputs cannot be pre-populated
            };

            TempData.Keep();
            return View("Entry", model);
        }

        [HttpGet]
        public IActionResult Entry()
        {
            _logger.LogInformation("Rendering Entry form at {Time}", DateTime.UtcNow);
            return View(new Customer());
        }

        [HttpPost]
        public IActionResult Entry(Customer model)
        {
            _logger.LogInformation(
                "POST /Customer/Entry received for {Email} at {Time}",
                model.Email,
                DateTime.UtcNow
            );

            // 1) Basic model validation
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Validation failed for Customer Entry: {Errors}",
                    string.Join("; ", ModelState.Values
                                               .SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage))
                );
                return View(model);
            }

            // 2) Image-specific validation & temp-save
            if (model.Image != null)
            {
                // a) Mime-type check
                var permittedTypes = new[] { "image/jpeg", "image/png" };
                if (!permittedTypes.Contains(model.Image.ContentType))
                {
                    ModelState.AddModelError(
                        nameof(model.Image),
                        "Invalid file type. Only JPEG and PNG are allowed."
                    );
                }

                // b) Extension check
                var ext = Path.GetExtension(model.Image.FileName).ToLowerInvariant();
                var permittedExts = new[] { ".jpg", ".jpeg", ".png" };
                if (!permittedExts.Contains(ext))
                {
                    ModelState.AddModelError(
                        nameof(model.Image),
                        "Invalid file extension. Only .jpg, .jpeg, .png are allowed."
                    );
                }

                // c) File size check (max 2 MB)
                const long maxBytes = 2 * 1024 * 1024;
                if (model.Image.Length > maxBytes)
                {
                    ModelState.AddModelError(
                        nameof(model.Image),
                        "File size exceeds 2 MB limit."
                    );
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Image validation failed: {Errors}",
                        string.Join("; ", ModelState.Values
                                                   .SelectMany(v => v.Errors)
                                                   .Select(e => e.ErrorMessage))
                    );
                    return View(model);
                }

                // d) Save to temp folder
                try
                {
                    var tempPath = Path.Combine(_environment.WebRootPath, "uploads", "temp");
                    Directory.CreateDirectory(tempPath);

                    var tempFileName = Path.GetRandomFileName() + ext;
                    var tempFilePath = Path.Combine(tempPath, tempFileName);

                    using var stream = new FileStream(tempFilePath, FileMode.Create);
                    model.Image.CopyTo(stream);

                    TempData["ImagePath"] = $"/uploads/temp/{tempFileName}";
                    _logger.LogInformation(
                        "Image saved temporarily at {ImagePath}",
                        TempData["ImagePath"]
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving image temporarily for {Email}", model.Email);
                    ModelState.AddModelError("", "Failed to upload image. Please try again.");
                    return View(model);
                }
            }

            // 3) Store form values in TempData
            TempData["FirstName"] = model.FirstName;
            TempData["LastName"] = model.LastName;
            TempData["Address"] = model.Address;
            TempData["City"] = model.City;
            TempData["State"] = model.State;
            TempData["ZipCode"] = model.ZipCode;
            TempData["Phone"] = model.Phone;
            TempData["Email"] = model.Email;
            TempData["ConfirmEmail"] = model.ConfirmEmail;

            _logger.LogInformation(
                "Customer Entry data stored in TempData for {FirstName} {LastName}",
                model.FirstName,
                model.LastName
            );

            return RedirectToAction("Confirm");
        }

        [HttpGet]
        public IActionResult Confirm()
        {
            _logger.LogInformation("Rendering Confirm page at {Time}", DateTime.UtcNow);

            var model = new Customer
            {
                FirstName = TempData["FirstName"] as string ?? string.Empty,
                LastName = TempData["LastName"] as string ?? string.Empty,
                Address = TempData["Address"] as string ?? string.Empty,
                City = TempData["City"] as string ?? string.Empty,
                State = TempData["State"] as string ?? string.Empty,
                ZipCode = TempData["ZipCode"] as string ?? string.Empty,
                Phone = TempData["Phone"] as string ?? string.Empty,
                Email = TempData["Email"] as string ?? string.Empty,
                ConfirmEmail = TempData["ConfirmEmail"] as string ?? string.Empty
            };

            ViewBag.ImagePath = TempData["ImagePath"] as string;
            TempData.Keep();

            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmSubmit()
        {
            _logger.LogInformation("POST /Customer/ConfirmSubmit at {Time}", DateTime.UtcNow);

            var tempImageUrl = TempData["ImagePath"] as string;
            string? finalImagePath = null;
            byte[]? imageBytes = null;

            if (!string.IsNullOrEmpty(tempImageUrl))
            {
                try
                {
                    var tempPath = Path.Combine(_environment.WebRootPath, "uploads", "temp");
                    var finalPath = Path.Combine(_environment.WebRootPath, "uploads");
                    var fileName = Path.GetFileName(tempImageUrl);
                    var source = Path.Combine(tempPath, fileName);
                    var destination = Path.Combine(finalPath, fileName);

                    Directory.CreateDirectory(finalPath);
                    //System.IO.File.Move(source, destination);
                    System.IO.File.Copy(source, destination, overwrite: true);
                    System.IO.File.Delete(source); // Clean up temp file

                    finalImagePath = $"/uploads/{fileName}";
                    _logger.LogInformation("Image moved to {FinalImagePath}", finalImagePath);

                    imageBytes = System.IO.File.ReadAllBytes(destination);
                    _logger.LogInformation("Image read into byte[] ({Length} bytes)", imageBytes.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error moving or reading image file for customer entry");
                }
            }

            var customer = new Customer
            {
                FirstName = TempData["FirstName"] as string ?? string.Empty,
                LastName = TempData["LastName"] as string ?? string.Empty,
                Address = TempData["Address"] as string ?? string.Empty,
                City = TempData["City"] as string ?? string.Empty,
                State = TempData["State"] as string ?? string.Empty,
                ZipCode = TempData["ZipCode"] as string ?? string.Empty,
                Phone = TempData["Phone"] as string ?? string.Empty,
                Email = TempData["Email"] as string ?? string.Empty,
                ConfirmEmail = TempData["ConfirmEmail"] as string ?? string.Empty,
                ImagePath = finalImagePath,
                ImageData = imageBytes
            };

            _db.Customers.Add(customer);
            _db.SaveChanges();

            _logger.LogInformation("Customer persisted to database with ID {Id}", customer.Id);
            return RedirectToAction("Success");
        }

        [HttpGet]
        public IActionResult Success()
        {
            _logger.LogInformation("Rendering Success page at {Time}", DateTime.UtcNow);
            return View();
        }
    }
}
