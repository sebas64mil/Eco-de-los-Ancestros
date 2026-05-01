using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class EffectPreviewFire : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private DataPlayer dataPlayer;

    [Header("Colisiones")]
    [SerializeField] private LayerMask collisionMask;

    [Header("Configuración (ScriptableObject)")]
    [SerializeField] private TrajectorySettingsSO settings;

    [Header("Visual")]
    [SerializeField] private float startWidth = 0.05f;
    [SerializeField] private float endWidth = 0.02f;

    [Header("Suavizado")]
    [SerializeField] private float smoothSpeed = 10f;

    [SerializeField] private GameObject projectilePrefab;

    private LineRenderer lineRenderer;
    private float smoothStrength;
    private Vector3[] previousPositions;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        UpdateWidth();

        smoothStrength = Mathf.Lerp(
            smoothStrength,
            dataPlayer.currentStrength,
            Time.deltaTime * smoothSpeed
        );

        DrawTrajectory();
    }

    private bool GetProjectileBounce()
    {
        if (projectilePrefab != null &&
            projectilePrefab.TryGetComponent<IProjectile>(out var projectile))
        {
            return projectile.AllowBounce;
        }

        return false;
    }
    private void UpdateWidth()
    {
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
    }

    private void DrawTrajectory()
    {
        if (settings == null) return;

        Vector2 startPos = firePoint.position;
        Vector2 velocity = firePoint.up * smoothStrength;

        bool allowBounce = GetProjectileBounce(); 

        List<Vector2> points = TrajectoryCalculator2D.Calculate(
            startPos,
            velocity,
            settings,
            collisionMask,
            allowBounce
        );

        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }

        SmoothLine();
    }
    private void SmoothLine()
    {
        if (previousPositions == null || previousPositions.Length != lineRenderer.positionCount)
        {
            previousPositions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(previousPositions);
            return;
        }

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 current = lineRenderer.GetPosition(i);
            current = Vector3.Lerp(previousPositions[i], current, 0.5f);
            lineRenderer.SetPosition(i, current);
        }

        lineRenderer.GetPositions(previousPositions);
    }
}