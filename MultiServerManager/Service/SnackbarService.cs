using MaterialDesignThemes.Wpf;

namespace MultiServerManager.Service;

public class SnackbarService
{
    public SnackbarMessageQueue MessageQueue { get; init; } = new();
}