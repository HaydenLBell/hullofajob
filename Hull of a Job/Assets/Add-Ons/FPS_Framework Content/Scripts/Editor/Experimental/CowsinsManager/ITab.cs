#if UNITY_EDITOR
namespace hb.framework
{
    public interface ITab
    {
        string TabName { get; }
        int Order { get; }
        bool IsVisible { get; }
        void OnGUI();
        void StartTab();
    }
}
#endif
