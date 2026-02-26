/*===========================================================================*
 *  Header file for Atom Config File
 *  Project            : GGJ_Game_Audio
 *  Tool Ver.          : Ver.3.56.01
 *  ACF  Ver.          : Ver.1.38.0
 *  File Name          : GGJ_Game_Audio_acf.h
 *  File Size          : 67584 bytes
 *  Target             : Public
 *  Categories         : 6
 *  AISAC Controls     : 16
 *  Global AISACs      : 4
 *  Voice Limit Groups : 1
 *  DSP BUS Settings   : 1
 *  DSP BUS Settings Snapshot  : 2
 *  DSP BUS Name       : 6
 *  Game Variables     : 2
 *  Selectors          : 3
 *  REACTs             : 1
 *  Output Ports  : 0
 *  Project Comment    : 
 *===========================================================================*/

#define CRI_GGJ_GAME_AUDIO_ACF_NUM_CATEGORIES_PER_PLAYBACK (4)
#define CRI_GGJ_GAME_AUDIO_ACF_CATEGORYNUM (6)
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROLNUM (16)
#define CRI_GGJ_GAME_AUDIO_ACF_GLOBALAISACNUM (4)
#define CRI_GGJ_GAME_AUDIO_ACF_VOICELIMITGROUPNUM (1)
#define CRI_GGJ_GAME_AUDIO_ACF_DSPSETTINGNUM (1)
#define CRI_GGJ_GAME_AUDIO_ACF_DSPSETTINGSNAPSHOTNUM (2)
#define CRI_GGJ_GAME_AUDIO_ACF_DSPBUSNAMENUM (6)
#define CRI_GGJ_GAME_AUDIO_ACF_GAMEVARIABLENUM (2)
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORNUM (3)
#define CRI_GGJ_GAME_AUDIO_ACF_REACTNUM (1)
#define CRI_GGJ_GAME_AUDIO_ACF_OUTPUTPORTNUM (0)

/* Category List (Category ID) */
#define CRI_GGJ_GAME_AUDIO_ACF_CATEGORY_SE_GAMEPLAY          ( 0) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_CATEGORY_SE_UI                ( 5) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_CATEGORY_SE_CHECKPOINT        ( 1) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_CATEGORY_SE_VO                ( 2) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_CATEGORY_SE_PAINT_MONO        ( 3) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_CATEGORY_MUSIC_GROUP          ( 6) /*  */

/* AISAC Control List (AISAC Control ID) */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_CHECKPOINT_LOWCUT_FILTER_TRANSITION ( 0) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_HEALTH_BAR_PITCH     ( 1) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_MENU_MUSIC_LOWPASS   ( 2) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_03      ( 3) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_04      ( 4) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_05      ( 5) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_06      ( 6) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_07      ( 7) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_08      ( 8) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_09      ( 9) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_10      (10) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_11      (11) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_12      (12) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_13      (13) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_14      (14) /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_AISACCONTROL_AISACCONTROL_15      (15) /*  */

/* Global AISAC List (Global AISAC Name) */
#define CRI_GGJ_GAME_AUDIO_ACF_GLOBALAISAC_REVERB_SEND          "Reverb_Send" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_GLOBALAISAC_LOWPASS_CONTROL      "LowPass_Control" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_GLOBALAISAC_HEALTH_BAR_PITCH     "Health_Bar_Pitch" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_GLOBALAISAC_LOWPASS_MENU_MUSIC   "LowPass_Menu_Music" /*  */

/* Voice Limit Group (Voice Limit Group Index) */
#define CRI_GGJ_GAME_AUDIO_ACF_VOICELIMITGROUP_VOICELIMITGROUP_0    ( 0) /*  */

/* DspSetting List (DspSetting Name) */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPSETTING_MIXER_0              "Mixer_0" /*   */

/* DspSettingSnapshot List (DspSettingSnapshot Name) */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPSETTINGSNAPSHOT_MIXER_0_FX_ON                "FX_ON" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPSETTINGSNAPSHOT_MIXER_0_MIXER_DRY            "Mixer_Dry" /*  */

/* DspBusName List (DspBus Name) */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPBUSNAME_EFFECTS_BUS          "Effects_Bus" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPBUSNAME_MASTEROUT            "MasterOut" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPBUSNAME_MASTERWET            "MasterWet" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPBUSNAME_MUSIC_BUS            "Music_Bus" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPBUSNAME_REVERB_BUS           "Reverb_Bus" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_DSPBUSNAME_SFX_BUS              "Sfx_Bus" /*  */

/* Game Variable (Game Variable Name) */
#define CRI_GGJ_GAME_AUDIO_ACF_GAMEVARIABLE_DEFAULT              "Default " /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_GAMEVARIABLE_CHECKPOINT_MUSIC_TRANSITION "Checkpoint_Music_Transition " /*  */

/* Selector/Selector Label List (Selector/Selector Label Name) */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTOR_CHECKPOINT           "CHECKPOINT" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_CHECKPOINT_WIN                  "WIN" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_CHECKPOINT_LOSE                 "LOSE" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTOR_MUSIC_SWITCH         "MUSIC_SWITCH" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_MUSIC_SWITCH_TOBLOCKA             "ToBlockA" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_MUSIC_SWITCH_TOBLOCKB             "ToBlockB" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_MUSIC_SWITCH_TOBLOCKC             "ToBlockC" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_MUSIC_SWITCH_TOBLOCKD             "ToBlockD" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_MUSIC_SWITCH_TOBLOCKWIN           "ToBlockWin" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_MUSIC_SWITCH_TOBLOCKLOSE          "ToBlockLose" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTOR_HEALTH_BAR           "HEALTH_BAR" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_HEALTH_BAR_UP                   "UP" /*  */
#define CRI_GGJ_GAME_AUDIO_ACF_SELECTORLABEL_HEALTH_BAR_DOWN                 "DOWN" /*  */

