﻿using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "lang", menuName = "Languages", order = 1)]
public class LangObject : ScriptableObject
{
    public string _menu_play_time;
    public string _menu_play_moves;
    public string _menu_play_exit;
    public string _menu_play_credits;
    public string _menu_play_back;
    public string _menu_play_menu;
    public string _menu_play_next;
    public string _menu_play_repeate;
    
    public string _hud_score_label;
    public string _hud_level_label;
    public string _hud_play_time;
    public string _hud_play_moves;

    public string _popup_win_label;
    public string _popup_lose_label;
    public string _popup_score_label;
    public string _popup_best_label;
    public string _popup_total_label;

}