using UnityEngine;
using UnityEngine.Events;
using cowsins;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int totalObjects;
    [SerializeField] private int repairedObjects;
    [SerializeField] private float percentRepaired;

    [SerializeField] private TextMeshProUGUI completionText;
    [SerializeField] private Image completionFill;

    public float PercentRepaired => percentRepaired;

    public UnityEvent<float> OnProgressChanged;
    public UnityEvent OnAllRepaired;

    private IRepairTask[] tasks;
    private bool allRepairedFired;

    private void OnEnable() => RepairEvents.OnTaskStateChanged += Recount;
    private void OnDisable() => RepairEvents.OnTaskStateChanged -= Recount;

    private void Start()
    {
        tasks = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IRepairTask>()
            .ToArray();
        totalObjects = tasks.Length;
        Recount();
    }

    private void Recount()
    {
        if (tasks == null) return;

        repairedObjects = 0;
        foreach (IRepairTask t in tasks)
        {
            if (t is MonoBehaviour mb && mb != null && t.IsRepaired) repairedObjects++;
        }

        percentRepaired = totalObjects > 0
            ? (float)repairedObjects / totalObjects * 100f
            : 100f;

        OnProgressChanged?.Invoke(percentRepaired);

        if (completionText != null) completionText.text = Mathf.FloorToInt(percentRepaired) + "%";
        if (completionFill != null) completionFill.fillAmount = percentRepaired / 100f;

        if (repairedObjects == totalObjects && totalObjects > 0 && !allRepairedFired)
        {
            allRepairedFired = true;
            OnAllRepaired?.Invoke();
        }
    }
}