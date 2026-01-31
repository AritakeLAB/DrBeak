using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxFactor; // 0なら動かない、1ならカメラと同じ速度、0.5なら半分

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        // パララックス係数に応じて背景を動かす
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, 0, 0);
        lastCameraPosition = cameraTransform.position;
    }
}