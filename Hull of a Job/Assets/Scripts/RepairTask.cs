namespace cowsins
{
    public interface IRepairTask
    {
        bool IsRepaired { get; }
    }

    public static class RepairEvents
    {
        public static event System.Action OnTaskStateChanged;
        public static void Notify() => OnTaskStateChanged?.Invoke();
    }
}