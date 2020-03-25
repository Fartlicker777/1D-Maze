using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class OneDimensionalMaze : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public Material[] color;
    public KMSelectable Ball;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private List<int> Position = new List<int> {1,1,1,1,0,1,1,0,0,1,1,1,0,1,0,0,1,0,0,1,1,0,1,0,0,1,0,1,0,0,1,0,0,0,1,1,0,0,1,1,0,0,0,1,0,1,0,1,0,1,0,1,0,1,1,0,1,1,1,1,1,1,0,0,1,1,0,0,0,0,1,1,1,0,0,1,0,1,0,0};
    int StartingWeed = 0;
    string SN = "FATASSANSWERCONFIRMCONFIRMANSWERGUCCI";
    int Callitsomethingyoucanremember = -1;
    int You = 0;
    int Fuck = 0;
    int Submit = 0;

    void Awake () {
        GetComponent<KMBombModule>().OnActivate += Activate;
        moduleId = moduleIdCounter++;
        foreach (KMSelectable Button in Buttons) {
                    Button.OnInteract += delegate () { ButtonPress(Button); return false; };
                }
                Ball.OnInteract += delegate () { PressBall(); return false; };
    }
    
    // Use this for initialization
    void Start () {
      SN = Bomb.GetSerialNumber();
      Callitsomethingyoucanremember = (int)Char.GetNumericValue(SN[5]);
      StartingWeed = UnityEngine.Random.Range(0,Position.Count());
      You = StartingWeed;
      Fuck = Position[StartingWeed];
      Ball.GetComponent<MeshRenderer>().material = color[Fuck];
      Debug.LogFormat("[1D Maze #{0}] You started at cell {1}.",moduleId,(You));
	}
    
  void ButtonPress(KMSelectable Button){
    if (moduleSolved) {
      return;
    }
    Button.AddInteractionPunch();
    GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
    if (Button == Buttons[0]) {
      if (You == 79) {
        You = 0;
      }
      else {
        You += 1;
      }
      Debug.Log(You);
      Ball.GetComponent<MeshRenderer>().material = color[Position[You]];
    }
    else if (Button == Buttons[1] && moduleSolved == false) {
      if (You == 0) {
        You = 79;
      }
      else {
        You -= 1;
      }
      Debug.Log(You);
      Ball.GetComponent<MeshRenderer>().material = color[Position[You]];
    }
    else{
      Debug.Log("FUUUCK");
      Debug.Log(You);
    }
  }
  
  void PressBall(){
        Ball.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (moduleSolved) {
          return;
        }
        for (int i = 0; i < 10; i++) {
        	if (Callitsomethingyoucanremember == i) {
        		if (You % 10 == i) {
        			  GetComponent<KMBombModule>().HandlePass();
        			  Debug.LogFormat("[1D Maze #{0}] You submitted at {1}. Module disarmed.",moduleId,(You));
        			  Audio.PlaySoundAtTransform("Farfare", transform);
        			  moduleSolved = true;
        			  Ball.GetComponent<MeshRenderer>().material = color[2];
        			}
        			else {
        			  GetComponent<KMBombModule>().HandleStrike();
        			  Debug.LogFormat("[1D Maze #{0}] You submitted at {1}. Strike, poopyhead.",moduleId,(You));
        			  Ball.GetComponent<MeshRenderer>().material = color[3];
        		}
        	}
        }
    }

    void Activate(){
      Audio.PlaySoundAtTransform("Activate", transform);
    }
}
