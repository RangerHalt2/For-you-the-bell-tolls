using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineCamera[] allCameras;

    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer positionComposer;

    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        for (int i = 0; i < allCameras.Length; i++)
        {
            if (allCameras[i].enabled)
            { 
                currentCamera = allCameras[i];

            }
        }

    }

    private void Start()
    {
        for (int i = 0; i < allCameras.Length; i++) 
        {
            allCameras[i].Follow = PlayerController.Instance.transform;
        }
    }

    public void SwapCamera(CinemachineCamera _newCam)
    {
        currentCamera.enabled = false;
        currentCamera = _newCam;
        currentCamera.enabled = true;
    }
}
