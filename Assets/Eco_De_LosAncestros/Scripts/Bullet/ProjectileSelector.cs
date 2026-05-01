using UnityEngine;
using System.Collections.Generic;

public class ProjectileSelector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ProjectileShooter shooter;

    [Header("Prefabs")]
    [SerializeField] private GameObject basicProjectile;
    [SerializeField] private GameObject embeddedProjectile;
    [SerializeField] private GameObject splitProjectile;
    [SerializeField] private GameObject areaProjectile;

    [Header("Containers")]
    [SerializeField] private Transform basicContainer;
    [SerializeField] private Transform embeddedContainer;
    [SerializeField] private Transform splitContainer;
    [SerializeField] private Transform areaContainer;

    [Header("Config")]
    [SerializeField] private int poolSize = 10;

    private Dictionary<GameObject, ObjectPool> pools = new();

    private Dictionary<GameObject, float> lockoutRemaining = new();

    private bool isSpecialActive;

    private GameObject currentSpecialPrefab;

    private void OnEnable()
    {
        InputHandler.OnCancelSpecial += CancelSpecial;

        InputHandler.OnEmbeddedProjectile += SetEmbedded;
        InputHandler.OnSplitProjectile += SetSplit;
        InputHandler.OnAreaProjectile += SetArea;

        ProjectileShooter.OnSpecialFired += CancelSpecial;
    }

    private void OnDisable()
    {
        InputHandler.OnCancelSpecial -= CancelSpecial;

        InputHandler.OnEmbeddedProjectile -= SetEmbedded;
        InputHandler.OnSplitProjectile -= SetSplit;
        InputHandler.OnAreaProjectile -= SetArea;

        ProjectileShooter.OnSpecialFired -= CancelSpecial;
    }

    private void Start()
    {
        lockoutRemaining[basicProjectile] = 0f;
        lockoutRemaining[embeddedProjectile] = 0f;
        lockoutRemaining[splitProjectile] = 0f;
        lockoutRemaining[areaProjectile] = 0f;

        SetBasic();
    }

    private void Update()
    {
        var keys = new List<GameObject>(lockoutRemaining.Keys);
        foreach (var prefab in keys)
        {
            if (lockoutRemaining[prefab] > 0f)
            {
                float prev = lockoutRemaining[prefab];
                lockoutRemaining[prefab] = Mathf.Max(0f, lockoutRemaining[prefab] - Time.deltaTime);

                if (prev > 0f && lockoutRemaining[prefab] <= 0f)
                {
                    Debug.Log($"{prefab.name} listo para seleccionar");
                }
            }
        }
    }

    private void SetBasic()
    {
        isSpecialActive = false;
        SetProjectile(basicProjectile, basicContainer);
    }

    private void SetEmbedded()
    {
        if (isSpecialActive) return;

        if (IsLocked(embeddedProjectile))
        {
            Debug.Log($"No puedes seleccionar {embeddedProjectile.name}. Falta {lockoutRemaining[embeddedProjectile]:F1}s");
            return;
        }

        isSpecialActive = true;
        SetProjectile(embeddedProjectile, embeddedContainer);
    }

    private void SetSplit()
    {
        if (isSpecialActive) return;

        if (IsLocked(splitProjectile))
        {
            Debug.Log($"No puedes seleccionar {splitProjectile.name}. Falta {lockoutRemaining[splitProjectile]:F1}s");
            return;
        }

        isSpecialActive = true;
        SetProjectile(splitProjectile, splitContainer);
    }

    private void SetArea()
    {
        if (isSpecialActive) return;

        if (IsLocked(areaProjectile))
        {
            Debug.Log($"No puedes seleccionar {areaProjectile.name}. Falta {lockoutRemaining[areaProjectile]:F1}s");
            return;
        }

        isSpecialActive = true;
        SetProjectile(areaProjectile, areaContainer);
    }

    private void CancelSpecial()
    {
        if (!isSpecialActive) return;

        var prevSpecial = currentSpecialPrefab;
        float cd = GetCooldownForPrefab(prevSpecial);
        if (prevSpecial != null)
        {
            lockoutRemaining[prevSpecial] = cd;
            Debug.Log($"Proyectil {prevSpecial.name} bloqueado por {cd:F1}s antes de poder seleccionarlo de nuevo");
        }

        SetBasic();
    }

    private bool IsLocked(GameObject prefab)
    {
        return lockoutRemaining.ContainsKey(prefab) && lockoutRemaining[prefab] > 0f;
    }

    private float GetCooldownForPrefab(GameObject prefab)
    {
        if (prefab == null) return 0f;

        if (prefab.TryGetComponent<IProjectile>(out var proj))
            return proj.FireCooldown;

        return 0.5f;
    }

    private void SetProjectile(GameObject prefab, Transform container)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new ObjectPool(prefab, poolSize, container);
        }

        Debug.Log($"Seleccionando proyectil: {prefab.name}");

        if (prefab == basicProjectile)
            currentSpecialPrefab = null;
        else
            currentSpecialPrefab = prefab;

        shooter.Initialize(prefab, pools[prefab]);
    }
}