    using Business.Abstracts;
    using Business.Requests.Domain;
    using Business.Responses.Domain;
    using Microsoft.AspNetCore.Mvc;

    namespace MailSender.Controllers
    {

        public class DomainController : Controller
        {
            private readonly IDomainService _domainService;
            public DomainController(IDomainService domainService)
            {
                _domainService = domainService;
            }
            [HttpGet("GetList")]
            public GetDomainListResponse GetList([FromQuery] GetDomainListRequest request)
            {
                GetDomainListResponse response = _domainService.GetList(request);
                return response;
            }
            [HttpGet("{Id}")] 
            public GetDomainByIdResponse GetById([FromRoute] GetDomainByIdRequest request)
            {
                GetDomainByIdResponse response = _domainService.GetById(request);
                return response;
            }

            [HttpPost("Add")]
            public ActionResult<AddDomainResponse> Add(AddDomainRequest request)
            {
                AddDomainResponse response = _domainService.Add(request);
                return CreatedAtAction(nameof(GetList), response);
            }
            [HttpPut("Update")]
            public void Update(UpdateDomainRequest request)
            {
                _domainService.Update(request);
            }
            [HttpDelete("Delete")]
            public void Delete(DeleteDomainRequest request)
            {
                _domainService.Delete(request);
            }
            [HttpPost("import")]
            public async Task<IActionResult> ImportDomains(IFormFile file)
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                try
                {
                    var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");


                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    var filePath = Path.Combine(uploadDirectory, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    await _domainService.ImportDomainsAsync(filePath);

                    return Ok("Domains imported successfully.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        [HttpGet("export")]
        public async Task<IActionResult> ExportDomains()
        {
            try
            {
                
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "DomainsExport.xlsx");

                var exportedFilePath = await _domainService.ExportDomainsToExcelAsync();

                var fileBytes = System.IO.File.ReadAllBytes(exportedFilePath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DomainsExport.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }

    }
    }
