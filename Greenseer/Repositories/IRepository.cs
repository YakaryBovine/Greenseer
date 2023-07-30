namespace Greenseer.Repositories;

public interface IRepository<T>
{
  Task Create(T player);
  Task Update(string id, T player);
  Task<T?> Get(string id);
  Task<List<T>> GetAll();
}