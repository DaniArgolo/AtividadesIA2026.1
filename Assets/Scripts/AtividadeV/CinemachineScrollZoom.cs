using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CinemachineScrollZoom : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    [Header("Zoom Ortográfico")]
    [SerializeField] private float orthographicSizeMin = 4f;
    [SerializeField] private float orthographicSizeMax = 14f;
    [SerializeField] private float velocidadeZoomOrtografico = 2f;

    [Header("Zoom Perspectiva")]
    [SerializeField] private float fieldOfViewMin = 25f;
    [SerializeField] private float fieldOfViewMax = 70f;
    [SerializeField] private float velocidadeZoomPerspectiva = 4f;

    [Header("Suavização")]
    [SerializeField] private bool usarSuavizacao = true;
    [SerializeField] private float velocidadeSuavizacao = 12f;

    private float alvoOrthographicSize;
    private float alvoFieldOfView;

    private void Reset()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private void Awake()
    {
        if (cinemachineCamera == null)
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        if (cinemachineCamera != null)
        {
            alvoOrthographicSize = cinemachineCamera.Lens.OrthographicSize;
            alvoFieldOfView = cinemachineCamera.Lens.FieldOfView;
        }
    }

    private void Update()
    {
        if (cinemachineCamera == null)
        {
            return;
        }

        LerScrollWheel();
        AplicarZoom();
    }

    private void LerScrollWheel()
    {
        if (Mouse.current == null)
        {
            return;
        }

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) < 0.01f)
        {
            return;
        }

        if (cinemachineCamera.Lens.Orthographic)
        {
            alvoOrthographicSize -= scroll * velocidadeZoomOrtografico * Time.deltaTime;

            alvoOrthographicSize = Mathf.Clamp(
                alvoOrthographicSize,
                orthographicSizeMin,
                orthographicSizeMax
            );
        }
        else
        {
            alvoFieldOfView -= scroll * velocidadeZoomPerspectiva * Time.deltaTime;

            alvoFieldOfView = Mathf.Clamp(
                alvoFieldOfView,
                fieldOfViewMin,
                fieldOfViewMax
            );
        }
    }

    private void AplicarZoom()
    {
        LensSettings lens = cinemachineCamera.Lens;

        if (lens.Orthographic)
        {
            if (usarSuavizacao)
            {
                lens.OrthographicSize = Mathf.Lerp(
                    lens.OrthographicSize,
                    alvoOrthographicSize,
                    velocidadeSuavizacao * Time.deltaTime
                );
            }
            else
            {
                lens.OrthographicSize = alvoOrthographicSize;
            }
        }
        else
        {
            if (usarSuavizacao)
            {
                lens.FieldOfView = Mathf.Lerp(
                    lens.FieldOfView,
                    alvoFieldOfView,
                    velocidadeSuavizacao * Time.deltaTime
                );
            }
            else
            {
                lens.FieldOfView = alvoFieldOfView;
            }
        }

        cinemachineCamera.Lens = lens;
    }
}