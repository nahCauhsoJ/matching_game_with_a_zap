using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;

public class Scoreboard : MonoBehaviour
{
    public float game_time_given = 120f;
    public float discharge_time_given = 10f;

    public static Scoreboard main;
    public static bool game_started;
    public static bool discharge_started; // The discharge timer is mostly managed by CardCore.
    public static float game_time_left; // The timer of the game
    // Note that discharge_time_left doesn't sync with game_time_left, since it only activates when a match is found.
    public static float discharge_time_left; // The time (in seconds) left till next discharge event
    public static int score;

    public Text game_timer_text;
    public Text discharge_timer_text;
    public TMP_Text score_text;
    public Image game_timer_img;
    public Image discharge_timer_img;
    public PlayableDirector game_over_timeline;
    
    void Start()
    {
        main = this;
        game_time_left = game_time_given;
        discharge_time_left = 0;
        score = 0;
    }

    void Update()
    {
        game_timer_text.text = Mathf.CeilToInt(game_time_left).ToString();
        discharge_timer_text.text = Mathf.CeilToInt(discharge_time_left).ToString();
        score_text.text = score.ToString();
        if (game_started)
        {
            if (game_time_left > 0) {game_time_left -= Time.deltaTime; if (game_time_left < 0) game_time_left = 0;}
            if (discharge_started && discharge_time_left > 0) {discharge_time_left -= Time.deltaTime; if (discharge_time_left < 0) discharge_time_left = 0;}
            if (game_time_left == 0) GameOver();
        }
    }

    void GameOver()
    {
        game_over_timeline.Play();
    }
}
