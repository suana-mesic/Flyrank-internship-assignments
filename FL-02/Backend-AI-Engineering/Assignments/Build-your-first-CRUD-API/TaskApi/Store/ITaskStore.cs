using TaskApi.Models;

namespace TaskApi.Store;

public interface ITaskStore
{
    List<TaskItem> GetAll();
    TaskItem? GetById(int id);
    TaskItem Add(string title);
    TaskItem? Update(int id, string? title, bool? done);
    bool Delete(int id);
    void Reset();
}
