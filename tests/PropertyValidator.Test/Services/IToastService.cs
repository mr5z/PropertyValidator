namespace PropertyValidator.Test.Services
{
    public interface IToastService
    {
        void ShowMessage(string message, params string[] args);
    }
}
