using AutoMapper;
using Business.Abstracts;
using Business.Requests.Domain;
using Business.Responses.Domain;
using DataAccess.Abstracts;
using Entities;
using OfficeOpenXml;



namespace Business.Concretes
{
    public class DomainManager : IDomainService
    {
        private readonly IDomainDal _domainDal;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        public DomainManager(IDomainDal domainDal, IMapper mapper, IMailService mailService)
        {
            _domainDal = domainDal;
            _mapper = mapper;
            _mailService = mailService;
        }

        public AddDomainResponse Add(AddDomainRequest request)
        {
            Domain domainToAdd = _mapper.Map<Domain>(request);
            _domainDal.Add(domainToAdd);
            AddDomainResponse response = _mapper.Map<AddDomainResponse>(domainToAdd);
            return response;
        }

        public DeleteDomainResponse Delete(DeleteDomainRequest request)
        {
            Domain? domainToDelete = _domainDal.Get(predicate: a => a.Id == request.Id);

            if (domainToDelete == null)
            {
                throw new KeyNotFoundException($"No Domain found with ID {request.Id}");
            }


            domainToDelete.IsDeleted = true;


            Domain updatedDomain = _domainDal.Update(domainToDelete);
            var response = _mapper.Map<DeleteDomainResponse>(updatedDomain);

            return response;
        }

        public GetDomainByIdResponse GetById(GetDomainByIdRequest request)
        {
            Domain? domain = _domainDal.Get(predicate: a => a.Id == request.Id);
            var response = _mapper.Map<GetDomainByIdResponse>(domain);
            return response;
        }

        public GetDomainListResponse GetList(GetDomainListRequest request)
        {
            IList<Domain> domains = _domainDal.GetList(predicate: a => a.IsDeleted == false);
            var domainsToNotify = domains.Where(d => d.EndTime != null && d.EndTime <= DateTime.Now.AddDays(15)).ToList();

            if (domainsToNotify.Any())
            {

                var subject = "Yaklaşan Domain Yenileme Tarihleri";
                var body = "Aşağıda, yenileme süresi yaklaşan domainler yer almaktadır:\n\n";

                foreach (var domain in domainsToNotify)
                {
                    body += $"Domain: {domain.DomainName}, Bitiş Tarihi: {domain.EndTime:yyyy-MM-dd}, Satın Alındığı Site: {domain.BuyedDomainSite}\n";
                }

                var mailRequest = new MailRequest
                {
                    ToEmail = "mehmetsimbil@icloud.com", 
                    Subject = subject,
                    Body = body
                };

                _mailService.SendEmailAsync(mailRequest);
            }

            GetDomainListResponse response = _mapper.Map<GetDomainListResponse>(domains);
            return response;
        }



        public UpdateDomainResponse Update(UpdateDomainRequest request)
        {
            Domain? domainToUpdate = _domainDal.Get(predicate: a => a.Id == request.Id);
            domainToUpdate = _mapper.Map(request, domainToUpdate);
            Domain updatedDomain = _domainDal.Update(domainToUpdate!);
            var response = _mapper.Map<UpdateDomainResponse>(updatedDomain);
            return response;
        }
        public async Task ImportDomainsAsync(string filePath)
        {
            var domains = new List<Domain>();


            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; 
                int rowCount = worksheet.Dimension.End.Row; 

                for (int row = 2; row <= rowCount; row++) 
                {
                    var domain = new Domain
                    {
                        DomainName = worksheet.Cells[row, 1].Text,
                        BuyedDomainSite = worksheet.Cells[row, 2].Text, 
                        EndTime = DateTime.Parse(worksheet.Cells[row, 3].Text) 
                    };


                    var existingDomain = _domainDal.Get(d => d.DomainName == domain.DomainName);

                    if (existingDomain != null)
                    {
                        existingDomain.BuyedDomainSite = domain.BuyedDomainSite;
                        existingDomain.EndTime = domain.EndTime;

                     _domainDal.Update(existingDomain);
                    }
                    else
                    {

                        domains.Add(domain);
                    }
                }
            }


            foreach (var domain in domains)
            {
               _domainDal.Add(domain); 
            }
        }
        public async Task<string> ExportDomainsToExcelAsync()
        {
            var domains = _domainDal.GetList(d => !d.IsDeleted);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Domains");

 
                worksheet.Cells[1, 1].Value = "Domain Name";
                worksheet.Cells[1, 2].Value = "Buyed Domain Site";
                worksheet.Cells[1, 3].Value = "End Time";

                for (int i = 0; i < domains.Count; i++)
                {
                    var domain = domains[i];
                    worksheet.Cells[i + 2, 1].Value = domain.DomainName;
                    worksheet.Cells[i + 2, 2].Value = domain.BuyedDomainSite;
                    worksheet.Cells[i + 2, 3].Value = domain.EndTime.ToString("yyyy-MM-dd");
                }
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Domains.xlsx");
                var fileInfo = new FileInfo(filePath);
                await package.SaveAsAsync(fileInfo);

                return filePath; 
            }
        }
    }
}
