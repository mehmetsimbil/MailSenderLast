using DataAccess.Abstracts;
using DataAccess.Context;
using Entities;


namespace DataAccess.Concretes
{
    public class EfDomainDal : EfRepositoryBase<Domain, ProjectContext>, IDomainDal
    {
        public EfDomainDal(ProjectContext context) : base(context)
        {
        }

    }
}
