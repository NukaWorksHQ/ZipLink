namespace Server.Common
{
    public interface IGenericService<ENTITY, C, U>
        where ENTITY : class
        where C : class
        where U : class
    {
        Task<IEnumerable<ENTITY>> GetAll();
        Task<ENTITY> Get(string id);
        Task<ENTITY> Edit(string id, U dto);
        Task<ENTITY> Create(C dto);
        Task<bool> Delete(string id);
        bool EntityExists(string id);
    }
}
