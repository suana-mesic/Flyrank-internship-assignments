using TaskApi.Models;

namespace TaskApi.Store;

// The "database" for this assignment: a plain list in memory. Fast and simple,
// but everything is gone when the process stops - that is the lesson, not a bug.
public class InMemoryTaskStore : ITaskStore
{
    private readonly List<TaskItem> _tasks = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    public InMemoryTaskStore() => Reset();

    public List<TaskItem> GetAll()
    {
        lock (_lock) return _tasks.ToList();
    }

    public TaskItem? GetById(int id)
    {
        lock (_lock) return _tasks.FirstOrDefault(t => t.Id == id);
    }

    public TaskItem Add(string title)
    {
        lock (_lock)
        {
            var task = new TaskItem { Id = _nextId++, Title = title, Done = false };
            _tasks.Add(task);
            return task;
        }
    }

    public TaskItem? Update(int id, string? title, bool? done)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null) return null;

            if (title is not null) task.Title = title;
            if (done is not null) task.Done = done.Value;
            return task;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null) return false;

            _tasks.Remove(task);
            return true;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _tasks.Clear();
            _tasks.AddRange(new[]
            {
                new TaskItem { Id = 1, Title = "Read the assignment brief", Done = true },
                new TaskItem { Id = 2, Title = "Build the CRUD API", Done = false },
                new TaskItem { Id = 3, Title = "Push to GitHub", Done = false }
            });
            _nextId = 4;
        }
    }
}
