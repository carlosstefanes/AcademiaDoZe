using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Messages;

// Transmite o tema selecionado ("Light", "Dark" ou "System")
public class ThemeChangedMessage : ValueChangedMessage<string>
{
    public ThemeChangedMessage(string value) : base(value) { }
}