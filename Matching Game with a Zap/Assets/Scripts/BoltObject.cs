using UnityEngine;
using UnityEngine.UI;

// Object dimension: 500x2500. Ratio 1:5. height * fill amount = world unit.
public class BoltObject : MonoBehaviour
{
    public RectTransform bolt_rect;
    public Image bolt_image;

    public Vector3 orig_pos{get;set;}
    void Awake() {orig_pos = transform.position;}

    void OnEnable() { bolt_image.enabled = true; bolt_image.fillAmount = 0f; }
    void OnDisable() { bolt_image.enabled = false; }

    // Do note that due to us cropping the lightning, start_pos is accurate while end_pos is not.
    public void Move(Vector3 start_pos, Vector3 end_pos)
    {
        start_pos = new Vector3(start_pos.x,start_pos.y,orig_pos.z);
        end_pos = new Vector3(end_pos.x,end_pos.y,orig_pos.z);
        transform.position = start_pos;

        transform.LookAt(end_pos); // Looks weird, but it's a hard-coded fix to this obnoxious angle issue.
        transform.rotation = Quaternion.Euler((transform.rotation.eulerAngles.x-90f) * Vector3.forward);

        Vector3[] bolt_rect_world = new Vector3[4];
        bolt_rect.GetWorldCorners(bolt_rect_world);
        bolt_image.fillAmount = Vector3.Distance(start_pos,end_pos) / Vector3.Distance(bolt_rect_world[0],bolt_rect_world[1]);
    }
}
