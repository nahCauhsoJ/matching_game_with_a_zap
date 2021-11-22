using UnityEngine;

public class AnimatedMask : MonoBehaviour
{
    public SpriteMask mask;
    public Sprite current_mask_image;
    void Update() {mask.sprite = current_mask_image;}
}
