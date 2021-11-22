using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Card size reference:
//   Simulator: Samsung Galaxy S10+ (ratio 19:9)
//   Screen size: 3040|1440
//   Card sprite scale: 0.15
public class CardObject : MonoBehaviour
{
    public Button card_button;
    public Image card_tint_selected;
    public GameObject card_mesh;
    public SpriteRenderer card_sprite;
    public Animator card_animator;
    public AudioSource card_sound;
    public BoltObject card_bolt;

    public SpriteRenderer card_cover_bolt_sprite;
    public SpriteRenderer card_cover_scratch_sprite;
    public Animator card_cover_animator;
    public UnityEngine.Rendering.Universal.Light2D card_light;

    public bool flipping;  // This is for the animation window to notify the animation's progress.

    public string card_name{get;set;} // Cards are identified by their sprite name
    public bool was_selected{get;set;} // Storing it so that the DischargeEvent can target it.
    public bool charging{get;set;} // Public, since CardCore will modify it at DischargeEvent.
    
    bool is_selected;
    bool charged;
    
    public bool block_select;

    void Start()
    {
        Discharge();
    }

    // Lazy function to change the card. Uses card_refs from CardCore.
    // Make sure the card name exists in card_refs. Throwing a tandrum if not.
    public void ChangeCard(string cn)
    {
        card_name = cn;
        card_sprite.sprite = CardCore.card_refs[cn];
    }

    // charge exists becuz we also Deselect when a discharge happens. However, due to the
    //      delay of the charge animation, this exists to explicitly say that this
    //      deselection doesn't charge up the card.
    public void Deselect(bool charge = true)
    {
        is_selected = false;
        card_animator.SetBool("revealed",false);
        Sound.Play(card_sound,Sound.main.flick,0.3f,Random.Range(0.85f,0.9f));
        flipping = true; // While the animator enables this, it's 1 tick too late, hence this extra.
        if (charge) StartCoroutine(ChargeCardAfter(0.33333f));
    }

    public IEnumerator ChargeCardAfter(float after = 0f)
    {
        if (!charged)
        {
            charged = true;
            yield return new WaitForSeconds(after);
            Sound.Play(card_sound,Sound.main.charge_up,0.2f,Random.Range(0.95f,1.1f));
            card_cover_animator.SetTrigger("lit");
        }
        
    }

    public void Discharge()
    {
        was_selected = false;
        charged = false;
    }

    public void Zap()
    {
        card_animator.SetTrigger("zap");
        card_cover_animator.SetTrigger("unlit");
        Sound.Play(card_sound,Sound.main.fry,1f,Random.Range(0.95f,1.1f));
        Sound.Play(card_sound,Sound.main.flap,1f,Random.Range(0.95f,1.1f),1f);
        is_selected = false;
        card_animator.SetBool("revealed",false);
        Discharge();
    }

    // For button's use
    public void OnSelect()
    {
        if (is_selected) return;
        if (CardCore.block_select.Count > 0) return;
        if (block_select) return;
        if (Scoreboard.game_time_left <= 0) return;

        if (CardCore.SelectCard(this))
        {
            card_animator.SetBool("revealed",true);
            flipping = true; // While the animator enables this, it's 1 tick too late, hence this extra.
            Sound.Play(card_sound,Sound.main.flick,1f,Random.Range(0.95f,1.1f));
            is_selected = true;
            was_selected = true;
        }
    }
}
