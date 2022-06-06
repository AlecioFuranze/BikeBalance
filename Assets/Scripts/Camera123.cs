using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera123 : MonoBehaviour
{
    public enum CameraView { FirstPerson, SecondPerson, ThirdPerson }
    public Transform observationObject;
    public Transform camera1;
    public Transform camera2;
    public Transform camera3;
    public CameraView cameraView;

    private Vector3 offset;
    private Quaternion rotation;
    private Vector3 startingPos;
    void Start()
    {
        offset = camera3.position - camera3.parent.position;
        rotation = camera3.rotation;
        startingPos = camera3.position;
        setView();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            swithCamera();
        setView();
    }
    private void setView()
    {
        camera1.gameObject.SetActive(cameraView == CameraView.FirstPerson);
        camera2.gameObject.SetActive(cameraView == CameraView.SecondPerson);
        camera3.gameObject.SetActive(cameraView == CameraView.ThirdPerson);

        switch (cameraView)
        {
            case CameraView.FirstPerson:
                {
                    break;
                }
            case CameraView.SecondPerson:
                {
                    camera2.eulerAngles = new Vector3(camera2.eulerAngles.x, camera2.eulerAngles.y, 0);
                    break;
                }
            case CameraView.ThirdPerson:
                {
                    camera3.position = camera3.parent.position + offset;
                    //camera3.position = startingPos;
                    camera3.rotation = rotation;
                    break;
                }
        }
    }
    private void swithCamera()
    {
        switch (cameraView)
        {
            case CameraView.FirstPerson: cameraView = CameraView.SecondPerson; break;
            case CameraView.SecondPerson: cameraView = CameraView.ThirdPerson; break;
            case CameraView.ThirdPerson: cameraView = CameraView.FirstPerson; break;
        }

    }
}
