using Microsoft.AspNetCore.Mvc;

namespace Server.Common
{
    public interface IGenericService<ENTITY, C, U>
        where ENTITY : class
        where C : class 
        where U : class
    {
        Task<IEnumerable<ENTITY>> GetAll();
        Task<IActionResult> Get(string id);
        Task<IActionResult> Edit(string id, U dto);
        Task<IActionResult> Create(C dto);
        Task<IActionResult> Delete(string id);
        bool EntityExists(string id);
    }
}
