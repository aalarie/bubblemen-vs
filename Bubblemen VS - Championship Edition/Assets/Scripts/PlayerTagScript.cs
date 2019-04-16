using UnityEngine;

/// <summary>
/// Script to keep player tag facing the camera.</summary>
public class PlayerTagScript : MonoBehaviour {

    /// <summary>
    /// This function is called every frame, if the <c>MonoBehaviour</c> is enabled.</summary>
    private void Update() {
        // http://answers.unity3d.com/questions/181000/gui-text-always-facing-camera.html
        Vector3 v = Camera.main.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt(Camera.main.transform.position - v);
        transform.Rotate(0, 180, 0);
    }
}
