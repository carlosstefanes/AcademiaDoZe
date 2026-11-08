using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Messages;

// Transmite o provedor selecionado ("SQLite", "MySQL" ou "SQL Server")
public class DatabaseConfigChangedMessage : ValueChangedMessage<string>
{
    public DatabaseConfigChangedMessage(string value) : base(value) { }
}