using System.Collections.Generic;
using System.Threading.Tasks;
using VirtualBuddy.Domain.Project.Entities;
using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class GetTechnologies
    {
        private readonly IRepository _repository;

        public GetTechnologies(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Technology>> ExecuteAsync()
        {
            var list = await _repository.GetAllAsync<Technology>();
            return new List<Technology>(list);
        }
    }
}
