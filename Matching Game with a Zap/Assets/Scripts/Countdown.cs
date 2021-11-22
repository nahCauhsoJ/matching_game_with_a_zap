using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    public TMP_Text cd_text;
    public Image blackscreen_img;

    public void ChangeNum(string txt) {cd_text.text = txt;}
    public void StartGame()
    {
        cd_text.gameObject.SetActive(false);
        blackscreen_img.gameObject.SetActive(false);
        Scoreboard.game_started = true;
    }
}
