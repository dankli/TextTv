namespace TextTv.Shared.Infrastructure.Contracts
{
public interface ILocalSettingsProvider
    {
        object GetValue(string name);
        void SetValue(string name, object value);
    }
}
