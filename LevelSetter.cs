using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class LevelSetter : EditorWindow
{
    static List<RoundInfo> R_Info;

    [MenuItem("ChesstarMJW/Level Monster Editor")]
    static void ShowWindow()
    {
        LevelSetter window = EditorWindow.GetWindow<LevelSetter>(EditorPrefs.GetBool("ChesstarMJW", true), "Level Monster Editor ---- By MJW", true);
        window.minSize = new Vector2(600, 400);
        window.maxSize = new Vector2(1200, 800);
    }

    public static TextAsset LocalFileAsset;
    
    static int RoundNum = 0;
    static int ConfirmRoundNum = 0;

    static bool RoundNumConfirm;
    static bool RoundDataConfirm;
    static bool WaveConfirm;
    static bool MstConfirm;

    static Vector2 scrollPosition = Vector2.zero;

    void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label(" ");
        GUILayout.BeginHorizontal();
        LocalFileAsset = EditorGUILayout.ObjectField("LocalFireName:", LocalFileAsset, typeof(TextAsset), true, GUILayout.Width(400), GUILayout.ExpandWidth(false)) as TextAsset;
        if (LocalFileAsset != null && GUILayout.Button("Confirm Load"))
        {
            ReadAndSetData(LocalFileAsset.ToString());

        }
        GUILayout.EndHorizontal();
        GUILayout.Label(" ");

        GUILayout.Label("RoundInfo");
        GUILayout.BeginHorizontal();

        GUILayout.Label("RoundNums:", GUILayout.Width(Screen.width/8));
        if (!RoundNumConfirm)
            RoundNum = int.Parse(GUILayout.TextField(RoundNum.ToString(), GUILayout.Width(Screen.width / 8)));
        else
            GUILayout.Label(RoundNum.ToString(), GUILayout.Width(Screen.width / 8));

        GUILayout.Label("", GUILayout.Width(Screen.width / 4));
        if (!RoundDataConfirm && !WaveConfirm && !MstConfirm && GUILayout.Button(RoundNumConfirm ? "UnConfirm RoundNum" : "Confirm RoundNum"))
        {
            if (!RoundNumConfirm)
            {
                R_Info = new List<RoundInfo>();
                ConfirmRoundNum = RoundNum;
                for (int i = 0; i < ConfirmRoundNum; i++)
                {
                    R_Info.Add(new RoundInfo());
                }
            }
            RoundNumConfirm = !RoundNumConfirm;
        }
        if (RoundNumConfirm)
        {

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("RoundName");
            GUILayout.Label("RoundTime");
            GUILayout.Label("RoundWaves");
            GUILayout.EndHorizontal();
            
            for (int i = 0; i < ConfirmRoundNum; i++)
            {
              
                GUILayout.BeginHorizontal();
                R_Info[i].RoundName = "R" + (i + 1).ToString();
                GUILayout.Label("  " + R_Info[i].RoundName);
                if (!RoundDataConfirm)
                {
                    R_Info[i].RoundTime = float.Parse(GUILayout.TextField(R_Info[i].RoundTime.ToString()));
                    R_Info[i].RoundWaveNum = int.Parse(GUILayout.TextField(R_Info[i].RoundWaveNum.ToString()));
                }
                else
                {
                    GUILayout.Label(R_Info[i].RoundTime.ToString());
                    GUILayout.Label(R_Info[i].RoundWaveNum.ToString());
                }
                GUILayout.EndHorizontal();
            }

            if (!WaveConfirm && !MstConfirm && GUILayout.Button(RoundDataConfirm ? "UnConfirm Round Info" : "Confirm Round Info"))
            {
                if (!RoundDataConfirm)
                {
                    for (int i = 0; i < ConfirmRoundNum; i++)
                    {
                        R_Info[i].WavesInfos = new WaveInfo[R_Info[i].RoundWaveNum];

                        for (int j = 0; j < R_Info[i].RoundWaveNum; j++)
                        {
                            R_Info[i].WavesInfos[j] = new WaveInfo();
                        }
                    }
                }
                RoundDataConfirm = !RoundDataConfirm;
            }
            if (RoundDataConfirm)
            {
                GUILayout.Label("");
                GUILayout.Label("WaveInfo");
                for (int i = 0; i < ConfirmRoundNum; i++)
                {
                    GUILayout.Label("  " + R_Info[i].RoundName);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("WaveName");
                    GUILayout.Label("WaveTime");
                    GUILayout.Label("WaveMstCount");
                    GUILayout.EndHorizontal();
                    for (int j = 0; j < R_Info[i].RoundWaveNum; j++)
                    {
                        GUILayout.BeginHorizontal();
                        R_Info[i].WavesInfos[j].WaveName = "W" + (j + 1).ToString();
                        GUILayout.Label("    " + R_Info[i].WavesInfos[j].WaveName);
                        if (!WaveConfirm)
                        {
                            R_Info[i].WavesInfos[j].WaveTime = float.Parse(GUILayout.TextField(R_Info[i].WavesInfos[j].WaveTime.ToString()));
                            R_Info[i].WavesInfos[j].WaveMstNum = int.Parse(GUILayout.TextField(R_Info[i].WavesInfos[j].WaveMstNum.ToString()));
                        }
                        else
                        {
                            GUILayout.Label(R_Info[i].WavesInfos[j].WaveTime.ToString());
                            GUILayout.Label(R_Info[i].WavesInfos[j].WaveMstNum.ToString());
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                if (!MstConfirm && GUILayout.Button(WaveConfirm ? "UnConfirm Waves Info" : "Confirm Waves Info"))
                {
                    if (!WaveConfirm)
                    {
                        for (int i = 0; i < ConfirmRoundNum; i++)
                        {
                            for (int j = 0; j < R_Info[i].RoundWaveNum; j++)
                            {
                                R_Info[i].WavesInfos[j].MstInfos = new MstSpawnInfo[R_Info[i].WavesInfos[j].WaveMstNum];
                                for (int k = 0; k < R_Info[i].WavesInfos[j].WaveMstNum; k++)
                                {
                                    R_Info[i].WavesInfos[j].MstInfos[k] = new MstSpawnInfo();
                                }
                            }
                        }
                    }
                    WaveConfirm = !WaveConfirm;
                }

                if (WaveConfirm)
                {
                    GUILayout.Label("");
                    GUILayout.Label("MstInfo");
                    for (int i = 0; i < ConfirmRoundNum; i++)
                    {
                        GUILayout.Label("  " + R_Info[i].RoundName);
                        for (int j = 0; j < R_Info[i].RoundWaveNum; j++)
                        {
                            GUILayout.Label("    " + R_Info[i].WavesInfos[j].WaveName);
                            if (!MstConfirm)
                            {
                                for (int k = 0; k < R_Info[i].WavesInfos[j].WaveMstNum; k++)
                                {
                                    GUILayout.BeginHorizontal();
                                    R_Info[i].WavesInfos[j].MstInfos[k].MstWaveID = "M" + (k + 1).ToString();
                                    GUILayout.Label("      " + R_Info[i].WavesInfos[j].MstInfos[k].MstWaveID);
                                    R_Info[i].WavesInfos[j].MstInfos[k].EnemyType = (E_EnemyType)EditorGUILayout.EnumPopup("EnemyType: ", R_Info[i].WavesInfos[j].MstInfos[k].EnemyType);
                                    GUILayout.Label("SpawnTime:");
                                    R_Info[i].WavesInfos[j].MstInfos[k].SpawnTime = float.Parse(GUILayout.TextField(R_Info[i].WavesInfos[j].MstInfos[k].SpawnTime.ToString()));
                                    R_Info[i].WavesInfos[j].MstInfos[k].RoomToInit = (E_InitPos)EditorGUILayout.EnumPopup("RoomType: ", R_Info[i].WavesInfos[j].MstInfos[k].RoomToInit);
                                    R_Info[i].WavesInfos[j].MstInfos[k].GoTransport = GUILayout.Toggle(R_Info[i].WavesInfos[j].MstInfos[k].GoTransport, "GoTransport");
                                    GUILayout.Label("InitPos:");
                                    R_Info[i].WavesInfos[j].MstInfos[k].InitPos = int.Parse(GUILayout.TextField(R_Info[i].WavesInfos[j].MstInfos[k].InitPos.ToString()));

                                    GUILayout.EndHorizontal();
                                }
                            }
                            else
                            {
                                for (int k = 0; k < R_Info[i].WavesInfos[j].WaveMstNum; k++)
                                {
                                    GUILayout.BeginHorizontal();
                                    R_Info[i].WavesInfos[j].MstInfos[k].MstWaveID = "M" + (k + 1).ToString();
                                    GUILayout.Label("      " + R_Info[i].WavesInfos[j].MstInfos[k].MstWaveID);
                                    GUILayout.Label("EnemyType: " + R_Info[i].WavesInfos[j].MstInfos[k].EnemyType.ToString());
                                    GUILayout.Label("SpawnTime:");
                                    GUILayout.Label(R_Info[i].WavesInfos[j].MstInfos[k].SpawnTime.ToString());
                                    GUILayout.Label("RoomType: "+ R_Info[i].WavesInfos[j].MstInfos[k].RoomToInit.ToString());
                                    GUILayout.Toggle(R_Info[i].WavesInfos[j].MstInfos[k].GoTransport, "GoTransport");
                                    GUILayout.Label("InitPos:");
                                    GUILayout.TextField(R_Info[i].WavesInfos[j].MstInfos[k].InitPos.ToString());
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }
                    }

                    if(GUILayout.Button(MstConfirm ? "Reset Mst":"Set To GameObject"))
                    {
                        if (!MstConfirm)
                        {
                            MstSpawner x = GameObject.FindObjectOfType<MstSpawner>();
                            x.SetRoundInfo(R_Info);
                            //R_Info.Clear();
                            //RoundNumConfirm = false;
                            //RoundDataConfirm = false;
                            //WaveConfirm = false;
                            Debug.Log("Set Info Success");
                            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                            WriteInText();
                        }
                        MstConfirm = !MstConfirm;
                    }
                }
            }
        }
        GUILayout.EndScrollView();
    }

    static void ReadAndSetData(string localFileText)
    {
        R_Info = xmDataReader.ReadRoundData(localFileText);
        ConfirmRoundNum = R_Info.Count;
        Debug.Log(R_Info[0].WavesInfos[0].MstInfos[0].EnemyType.ToString());
        RoundNumConfirm = true;
        RoundDataConfirm = true;
        WaveConfirm = true;
        MstConfirm = true;
    }

    static void WriteInText()
    {
        xmDataReader.SetRoundData(R_Info);
    }
}
