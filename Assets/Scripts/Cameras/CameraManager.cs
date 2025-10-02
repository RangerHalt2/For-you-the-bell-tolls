using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineCamera[] allCameras;

    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer positionComposer;

    [Header("Y Damping Settings for Player Jump/Fall:")]
    [SerializeField] private float panAmount = 0.1f;
    [SerializeField] private float panTime = 0.2f;
    public float playerFallSpeedThreshold = -10;
    public bool isLerpingYDamping;
    public bool hasLerpedYDamping;

    private float normalYDamp;


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

                //currentCamera.PositionControl = CinemachineCamera.PositionControl.PositionComposer;
            }
        }

        //normalYDamp = positionComposer.Damping.y;
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

    public IEnumerator LerpYDamping(bool _isPlayerFalling) 
    {
        isLerpingYDamping = true;
        //take start y damp amount
        float _startYDamp = positionComposer.Damping.y;
        float _endYDamp = 0;
        //determine end damp amount
        if (_isPlayerFalling)
        {
            _endYDamp = panAmount;
            hasLerpedYDamping = true;
        }
        else 
        { 
            _endYDamp = normalYDamp;
        }
        //lerp panAmount
        float _timer = 0;
        while (_timer < panTime)
        {
            _timer += Time.deltaTime;
            float _lerpedPanAmount = Mathf.Lerp(_startYDamp, _endYDamp, (_timer / panTime));
            positionComposer.Damping.y = _lerpedPanAmount;
            yield return null;
        }
        isLerpingYDamping = false;
    }
}
