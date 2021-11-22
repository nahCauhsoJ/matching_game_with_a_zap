using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCore : MonoBehaviour
{
    public static CardCore main;
    public static Camera main_cam;

    public GameObject card_prefab;
    public Sprite card_cover_sprite;

    public AudioSource card_board_sound;

    // The index of the sprite is what stored in num in CardObject.
    [Header("These are the card variants")]
    public List<Sprite> card_sprites = new List<Sprite>();
    public List<CardObject> card_deck = new List<CardObject>();
    public List<Sprite> bolt_sprites = new List<Sprite>();
    public static Dictionary<string,Sprite> card_refs = new Dictionary<string,Sprite>();

    public static CardObject[] selected_cards = new CardObject[2];
    public static List<string> block_select = new List<string>();

    // These will be changed at runtime everytime a match occured. Well, and start of the game.
    List<string> unused_cards;
    List<string> used_cards;

    void Start()
    {   
        main = this;
        main_cam = Camera.main;
        foreach (var i in card_sprites) card_refs[i.name] = i;
        SpreadCards();
    }

    // Update is called once per frame
    void Update()
    {
        if (selected_cards[0] != null && selected_cards[1] != null &&
            !selected_cards[0].flipping && !selected_cards[1].flipping) ProcessMatching();

        if (Scoreboard.discharge_started && Scoreboard.discharge_time_left <= 0)
        {
            Scoreboard.discharge_started = false;
            StartCoroutine(DischargeEvent());
        }
    }

    // Let's hard-code 24 cards for now.
    public bool SpreadCards()
    {
        if (card_deck.Count % 2 != 0) return false;

        List<string> card_name_shuffle = new List<string>(card_refs.Keys);
        int card_name_needed = card_deck.Count / 2;
        List<string> card_names = new List<string>();
        for (var i = 0; i < card_name_needed; i++)
        {
            int rng = Random.Range(0,card_name_shuffle.Count);
            card_names.Add(card_name_shuffle[rng]);
            card_name_shuffle.RemoveAt(rng);
        }
        List<string> card_deck_names = new List<string>(card_names);
        card_deck_names.AddRange(card_names);
        for (var i = 0; i < 100; i++)
        {  // This basically moves a random card in deck to the deck top, repeated 100 times.
            int rng = Random.Range(0,card_deck_names.Count);
            string card_top = card_deck_names[rng];
            card_deck_names.RemoveAt(rng);
            card_deck_names.Add(card_top);
        }

        // Don't worry, card_deck_names and card_deck are guaranteed to have the same count.
        for (var i = 0; i < card_deck.Count; i++) card_deck[i].ChangeCard(card_deck_names[i]);

        // Here's where we'll sort all names as used or unused in the game board.
        used_cards = card_names;
        List<string> card_name_remaining = new List<string>();
        foreach (var i in card_refs.Keys) if (!card_names.Contains(i)) card_name_remaining.Add(i);
        unused_cards = card_name_remaining;

        return true;
    }

    // The function to process the moment we click on a card.
    // The matching only occurs after both cards are done flipping, which is handled by Update().
    public static bool SelectCard(CardObject card)
    {
        if (selected_cards[0] == null) selected_cards[0] = card;
        else if (selected_cards[1] == null) selected_cards[1] = card;
        else return false;
        return true;
    }

    // Lazy function to perform a deselect. Happens when not matching or time to swap cards.
    public static void DeselectCard(bool charge = true)
    {
        if (selected_cards[0] != null) selected_cards[0].Deselect(charge);
        if (selected_cards[1] != null) selected_cards[1].Deselect(charge);
        selected_cards = new CardObject[2];
    }

    // If old_bolt is given, the returned sprite will never match the old bolt, unless there's only one.
    public Sprite RandomBolt(Sprite old_bolt = null)
    {
        if (old_bolt != null)
        {
            int rng = Random.Range(0,bolt_sprites.Count-1);
            if (bolt_sprites[rng] == old_bolt) return bolt_sprites[bolt_sprites.Count-1];
            else return bolt_sprites[rng];
        } else return bolt_sprites[Random.Range(0,bolt_sprites.Count)];
    }

    // The function to see if the cards match, and act on the results.
    // It's made sure that exactly 2 cards were selected when running this.
    public void ProcessMatching()
    {
        if (selected_cards[0].card_name == selected_cards[1].card_name)
        {
            selected_cards[0].Zap();
            selected_cards[1].Zap();
            selected_cards[0].block_select = true;
            selected_cards[1].block_select = true;

            Scoreboard.score += Scoreboard.discharge_time_left > 0 ? 15 : 10;
            // This means that the zap event won't reset if another match is found during it.
            if (!Scoreboard.discharge_started)
            {
                Scoreboard.discharge_started = true;
                Scoreboard.discharge_time_left = Scoreboard.main.discharge_time_given;
            }

            // This is to pick any 2 cards from the board that is not the currently zapped cards.
            List<CardObject> remaining_deck = new List<CardObject>(card_deck);
            remaining_deck.Remove(selected_cards[0]);
            remaining_deck.Remove(selected_cards[1]);
            CardObject[] picked_cards = new CardObject[2];
            int rng = Random.Range(0,remaining_deck.Count);
            picked_cards[0] = remaining_deck[rng];
            remaining_deck.RemoveAt(rng);
            picked_cards[1] = remaining_deck[Random.Range(0,remaining_deck.Count)];

            string zapped_card_name = selected_cards[0].card_name;
            string new_card_name = unused_cards[Random.Range(0,unused_cards.Count)];
            StartCoroutine(MatchedChangeCard(selected_cards,zapped_card_name,picked_cards,new_card_name));
            
            used_cards.Remove(zapped_card_name);
            used_cards.Add(new_card_name);
            unused_cards.Remove(new_card_name);
            unused_cards.Add(zapped_card_name);

            selected_cards = new CardObject[2];
        } else DeselectCard();
    }

    IEnumerator MatchedChangeCard(CardObject[] zapped_cards, string zapped_card_name, CardObject[] picked_cards, string new_card_name)
    {
        yield return new WaitForSeconds(0.5f);
        zapped_cards[0].ChangeCard(picked_cards[0].card_name);
        zapped_cards[1].ChangeCard(picked_cards[1].card_name);
        picked_cards[0].ChangeCard(new_card_name);
        picked_cards[1].ChangeCard(new_card_name);
        zapped_cards[0].block_select = false;
        zapped_cards[1].block_select = false;
    }

    IEnumerator DischargeEvent()
    {
        if (selected_cards[0] != null) DeselectCard(false);
        block_select.Add("discharge");

        HashSet<string> targeted_names = new HashSet<string>();
        List<CardObject> charged_cards = new List<CardObject>();
        List<CardObject> uncharged_cards = new List<CardObject>();
        List<CardObject> will_charge_cards = new List<CardObject>();
        foreach (var i in card_deck)
            if (i.was_selected) { charged_cards.Add(i); targeted_names.Add(i.card_name); }
            else uncharged_cards.Add(i);
        foreach (var i in uncharged_cards) if (targeted_names.Contains(i.card_name)) will_charge_cards.Add(i);
        Vector3 discharge_img_pos = Scoreboard.main.discharge_timer_img.transform.position;
        foreach (var i in will_charge_cards)
        {
            i.card_bolt.bolt_image.sprite = RandomBolt();
            i.card_bolt.gameObject.SetActive(true);
            i.card_bolt.Move(discharge_img_pos,i.card_bolt.orig_pos);
            StartCoroutine(i.ChargeCardAfter());
            i.was_selected = true;
        }

        yield return new WaitForSeconds(0.2f);
        foreach (var i in will_charge_cards) i.card_bolt.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.85f);

        Sound.Play(card_board_sound,Sound.main.charge_up_peak,0.5f);
        List<CardObject> remaining_deck = new List<CardObject>(card_deck);
        foreach (var i in card_deck) if (!i.was_selected) remaining_deck.Remove(i);
        foreach (var i in remaining_deck) {i.card_cover_animator.SetTrigger("charge");}
        
        yield return new WaitForSeconds(1.5f);
        Sound.Play(card_board_sound,Sound.main.discharge,0.5f);
        List<string> orig_deck_order = new List<string>();
        foreach (var i in remaining_deck)
        {
            orig_deck_order.Add(i.card_name);
            i.card_bolt.bolt_image.sprite = RandomBolt();
            i.card_bolt.gameObject.SetActive(true);
            i.card_bolt.Move(discharge_img_pos,i.card_bolt.orig_pos);
        }
        for (var i = 0; i < 16; i++)
        {
            foreach (var ii in remaining_deck) ii.card_bolt.bolt_image.sprite = RandomBolt(ii.card_bolt.bolt_image.sprite);
            yield return new WaitForSeconds(0.05f); // The sound lasts for 0.837s. 16 times should suffice.
        }
        for (var i = 0; i < 100; i++)
        {  // Similar as SpreadCard()'s loop, except it's not the whole deck.
            int rng = Random.Range(0,remaining_deck.Count);
            CardObject card_top = remaining_deck[rng];
            remaining_deck.RemoveAt(rng);
            remaining_deck.Add(card_top);
        }

        for (var i = 0; i < remaining_deck.Count; i++)
        {
            remaining_deck[i].card_bolt.gameObject.SetActive(false);
            remaining_deck[i].Discharge();
            remaining_deck[i].card_cover_animator.SetTrigger("discharge");
            remaining_deck[i].ChangeCard(orig_deck_order[i]);
        }
        block_select.Remove("discharge");
    }
}