using System.Collections.Generic;
using System.Linq;

namespace AcademiaDoZe.Domain;

public record Notification(string Mensagem);

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public IReadOnlyCollection<Notification> Notifications { get; }

    protected Result(bool isSuccess, T? value, string? error, IEnumerable<Notification>? notifications = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        var list = new List<Notification>();
        if (!string.IsNullOrWhiteSpace(error))
            list.Add(new Notification(error));
        if (notifications != null)
            list.AddRange(notifications);
        Notifications = list.AsReadOnly();
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> Failure(IEnumerable<Notification> notifications) => new(false, default, string.Join("; ", notifications.Select(n => n.Mensagem)), notifications);
}

